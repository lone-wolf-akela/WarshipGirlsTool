// WarshipGirlsPNGTool.h

#pragma once

#include <sstream>
#include <msclr\marshal_cppstd.h>
#include "Decrypt.h"

//using namespace std;

using namespace System;
using namespace System::Drawing;
using namespace System::Drawing::Imaging;
using namespace System::IO;
using namespace System::IO::Compression;
using namespace System::Text;
using namespace msclr::interop;

using std::stringstream;
using std::string;

#pragma pack(1)
struct CCZHeader 
{
	uint8_t	sig[4];				// signature. Should be 'CCZ!' 4 bytes
	uint16_t compression_type;	// should 0
	uint16_t version;			// should be 2 (although version type==1 is also supported)
	uint32_t reserved;			// Reserverd for users.
	uint32_t len;				// size of the uncompressed file
};
struct PVRHeader
{
	uint32_t headerSize;
	uint32_t height;
	uint32_t width;
	uint32_t mipmapLv;
	uint32_t pixelFormat;
	uint32_t dataSize;
	uint32_t bitsPerPixel;
	uint32_t redMask;
	uint32_t greenMask;
	uint32_t blueMask;
	uint32_t alphaMask;
	uint8_t pvrId[4];
	uint32_t surfaceNum;
};
#pragma pack()

#define SWAP16(x) (x)=( ((x)<<8) | ((x)>>8) )
#define SWAP32(x) (x)=( ((x)<<24) | (((x)<<8)&0x00ff0000) | (((x)>>8)&0x0000ff00) | ((x)>>24) )

namespace WarshipGirlsPNGTool 
{
	public ref class WSGPNG
	{
	public:
		static Image^ getShipModel(String ^filename);
		static Bitmap^ getPVRCCZ(String ^filename);		
	};
}
