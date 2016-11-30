// WarshipGirlsPNGTool.h

#pragma once

#include <sstream>
#include <msclr\marshal_cppstd.h>
#include "Decrypt.h"

using namespace std;

using namespace System;
using namespace System::Drawing;
using namespace System::IO;
using namespace System::Text;
using namespace msclr::interop;

namespace WarshipGirlsPNGTool {

	public ref class WSGPNG
	{
	public:
		static Image ^getShipModel(String ^filename)
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
			while(o_file.peek()!=EOF)
			{
				o_file.read((char*)&temp, sizeof(temp));
				o_stream->WriteByte(temp);
			}
			return Image::FromStream(o_stream);
		}
	};
}
