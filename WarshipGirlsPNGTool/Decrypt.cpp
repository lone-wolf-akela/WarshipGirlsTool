#include "stdafx.h"

#include "Decrypt.h"
#include "IceKey.h"
#include "CRC32.h"

//#include <Winsock2.h>
#include <memory>
#include <fstream>
#include <iostream>
#include <sstream>
#include <boost/algorithm/string.hpp>

#include <msclr\marshal_cppstd.h>

using namespace std;
using namespace System;
using namespace System::Text;
using namespace System::IO;
using namespace System::Security::Cryptography;
using namespace msclr::interop;

// 长整型大小端互换
#define BigLittleSwap32(A)  ((((uint32_t)(A) & 0xff000000) >> 24) | \
(((uint32_t)(A) & 0x00ff0000) >> 8) | \
(((uint32_t)(A) & 0x0000ff00) << 8) | \
(((uint32_t)(A) & 0x000000ff) << 24))
// 本机大端返回1，小端返回0
int checkCPUendian()
{
	union {
		uint32_t i;
		unsigned char s[4];
	}c;
	c.i = 0x12345678;
	return (0x12 == c.s[0]);
}
// 模拟ntohl函数，网络字节序转本机字节序
unsigned long int ntohl(uint32_t n)
{
	// 若本机为大端，与网络字节序同，直接返回
	// 若本机为小端，网络数据转换成小端再返回
	return checkCPUendian() ? n : BigLittleSwap32(n);
}


// 解密PNG图片
stringstream DecryptMuka(string filename)
{
	aes_key key = { 0 };
	memcpy(&key[0], "aabbccddeeff2333", key.size());

	ifstream in_file(filename, ios::binary | ios::ate);
	if (!in_file.is_open())
	{
		throw gcnew Exception(marshal_as<String^>("打开" + filename + "失败！"));
	}

	// 读取数据块位置
	uint32_t end_pos = uint32_t(in_file.tellg());
	in_file.seekg(end_pos - sizeof(uint32_t));
	uint32_t block_start_pos = ntohl(*reinterpret_cast<uint32_t *>(&(ReadSome<sizeof(uint32_t)>(in_file)[0])));
	in_file.seekg(block_start_pos);

	// 获取数据块大小
	const uint32_t block_size = ntohl(*reinterpret_cast<uint32_t *>(&ReadSome<sizeof(uint32_t)>(in_file)[0]));

	// 解密数据块信息
	auto block_info = ReadLarge(in_file, uint32_t(end_pos - block_start_pos - sizeof(uint32_t) * 2));
	try
	{
		DecryptBlock(block_info, key);
	}
	catch (std::string s)
	{
		throw gcnew Exception(marshal_as<String^>(filename + "解密失败！"));
	}

	// 验证校验和
	block_info.seekg(block_size);
	uint32_t crc32 = ntohl(*reinterpret_cast<uint32_t *>(&ReadSome<sizeof(uint32_t)>(block_info)[0]));
	if (crc32 != CRC32(block_info.str().substr(0, block_size)))
	{
		throw gcnew Exception(marshal_as<String^>(filename + "校验和验证失败！"));
	}

	// 创建PNG文件流
	stringstream out_file;

	// 写入文件头
	SteamCopy(out_file, HEAD_DATA, sizeof(HEAD_DATA));

	// 读取数据块
	block_info.seekg(0);
	uint64_t read_size = 0;
	while (true)
	{
		// 验证数据有效性
		if (block_info.tellg() >= block_size)
		{
			//out_file.clear();
			throw gcnew Exception(marshal_as<String^>(filename + "file format error!"));
		}

		// 读取数据块信息
		Block block;
		memcpy(&block, &ReadSome<sizeof(Block)>(block_info)[0], sizeof(Block));

		// 写入数据块长度
		SteamCopy(out_file, &block.size, sizeof(block.size));

		// 大小端转换
		block.pos = ntohl(block.pos);
		block.size = ntohl(block.size);

		// 写入数据块名称
		SteamCopy(out_file, &block.name, sizeof(block.name));

		// 写入数据块内容
		string s_name(block.name, sizeof(block.name));
		if (strcmp(s_name.c_str(), "IHDR") == 0)
		{
			IHDRBlock ihdr;
			memcpy(&ihdr, &block, sizeof(Block));
			memcpy(reinterpret_cast<char *>(&ihdr) + sizeof(Block), &ReadSome<sizeof(IHDRBlock) - sizeof(Block)>(block_info)[0], sizeof(IHDRBlock) - sizeof(Block));
			SteamCopy(out_file, ihdr.data, sizeof(ihdr.data));
		}
		else if (strcmp(s_name.c_str(), "IEND") == 0)
		{
			SteamCopy(out_file, IEND_DATA, sizeof(IEND_DATA));
			break;
		}
		else
		{
			in_file.seekg(read_size);
			StreamMove(out_file, in_file, block.size + CRC_SIZE);
			read_size += block.size + CRC_SIZE;
		}
	}
	return out_file;
}

// 解密mukaR PNG图片

stringstream DecryptMukaR(string filename)
{

	IceKey icekey(0);

	std::string keyword = boost::replace_last_copy(filename, ".mukaR", "");
	auto md5input = Encoding::UTF8->GetBytes(marshal_as<String^>("Zhanjian") 
		+ Path::GetFileName(marshal_as<String^>(keyword)));
	auto md5 = gcnew MD5CryptoServiceProvider();
	auto md5output = md5->ComputeHash(md5input);
	auto keytext = BitConverter::ToString(md5output)->Replace("-", "")->ToLower();
	auto key = Encoding::UTF8->GetBytes(keytext);
	pin_ptr<unsigned char> keyp = &key[0];
	icekey.set(keyp);

	std::ifstream in_file(filename, std::ios::binary | std::ios::ate);
	if (!in_file.is_open())
	{
		throw gcnew Exception(marshal_as<String^>("打开" + filename + "失败！"));
	}

	size_t length = in_file.tellg();
	in_file.seekg(0);

	// 创建PNG文件流
	stringstream out_file;
	std::unique_ptr<unsigned char[]> buffer_in(new unsigned char[length]);
	std::unique_ptr<unsigned char[]> buffer_out(new unsigned char[length]);

	in_file.read(reinterpret_cast<char*>(buffer_in.get()), length);

	for (size_t i = 0;i < length;i += 8)
	{
		icekey.decrypt(&buffer_in[i], &buffer_out[i]);
	}

	out_file.write(reinterpret_cast<char*>(buffer_out.get()), length);

	in_file.close();

	return out_file;
}