// 这是主 DLL 文件。

#include "stdafx.h"

#include "WarshipGirlsPNGTool.h"

using namespace WarshipGirlsPNGTool;

Image^ WSGPNG::getShipModel(String ^filename)
{
	stringstream o_file;

	if (Path::GetExtension(filename)->ToLower() == ".muka")
		o_file = DecryptMuka(marshal_as<string>(filename));
	else if (Path::GetExtension(filename)->ToLower() == ".mukar")
		o_file = DecryptMukaR(marshal_as<string>(filename));
	else
		throw gcnew Exception("Unknown File Type!");

	auto o_stream = gcnew MemoryStream;
	auto sw = gcnew StreamWriter(o_stream);
	o_file.seekg(0);
	unsigned char temp;
	while (o_file.peek() != EOF)
	{
		o_file.read((char*)&temp, sizeof(temp));
		o_stream->WriteByte(temp);
	}

	return Image::FromStream(o_stream);
}

Bitmap^ WSGPNG::getPVRCCZ(String ^filename)
{
	if (!filename->ToLower()->EndsWith(".pvr.ccz"))
		throw gcnew Exception("Not a PVR.CCZ File!");

	FileStream ^file = gcnew FileStream(filename, FileMode::Open, FileAccess::Read);

	array<unsigned char> ^cczHeaderBuffer = gcnew array<unsigned char>(sizeof CCZHeader);
	file->Read(cczHeaderBuffer, 0, sizeof CCZHeader);
	pin_ptr<unsigned char> pCCZHeader = &cczHeaderBuffer[0];
	CCZHeader *cczHeader = (CCZHeader*)pCCZHeader;
	SWAP16(cczHeader->compression_type);
	SWAP16(cczHeader->version);
	SWAP32(cczHeader->reserved);
	SWAP32(cczHeader->len);

	file->Position += 2;
	DeflateStream ^unzip = gcnew DeflateStream(file, CompressionMode::Decompress);

	array<unsigned char> ^pvrHeaderbuffer = gcnew array<unsigned char>(sizeof PVRHeader);
	unzip->Read(pvrHeaderbuffer, 0, sizeof PVRHeader);
	pin_ptr<unsigned char>pPVRHeader = &pvrHeaderbuffer[0];
	PVRHeader *pvrHeader = (PVRHeader*)pPVRHeader;

	array<unsigned char> ^imageBuffer = gcnew array<unsigned char>(pvrHeader->dataSize);
	unzip->Read(imageBuffer, 0, pvrHeader->dataSize);
	pin_ptr<unsigned char> pImage = &imageBuffer[0];

	for (uint32_t *pixel = (uint32_t*)pImage;pixel - (uint32_t*)pImage < pvrHeader->dataSize / 4;pixel++)
		*pixel = ((*pixel & 0xff00ff00) | ((*pixel & 0x000000ff) << 16) | ((*pixel & 0x00ff0000) >> 16));

	Bitmap ^image = gcnew Bitmap(pvrHeader->width, pvrHeader->height,
		4 * pvrHeader->width, PixelFormat::Format32bppArgb, IntPtr(pImage));

	return image;
}