key = "ON" --总开关 
--key = "OFF"

waifu2x_mode = "cudnn" --waifu2x显卡加速模式
--waifu2x_mode = "gpu"
--waifu2x_mode = "cpu"

function SplitFilename(strFilename)
	-- Returns the Path, Filename, and Extension as 3 values
	return string.match(strFilename, "(.-)([^\\]-([^\\%.]+))$")
end

function imageReplace(filestr, image, typestr)
	if key == "ON" then
		path,filestr,extension = SplitFilename(filestr)	
	
		dofile("mod\\shipModelList.lua")
		shipModelList = shipModelList or {}	
		if typestr == "shipmodel" then
			if not shipModelList[filestr] then
				shipModelList[filestr] = {}
				image:Save("mod\\temp.png")
				os.execute("mod\\waifu2x\\waifu2x-caffe-cui.exe -s 2 -n 0 -p "..waifu2x_mode.." --model_dir mod\\waifu2x\\models\\anime_style_art_rgb -m noise_scale -i mod\\temp.png -o mod\\res\\"..filestr..".png")
				os.remove("mod\\temp.png")
				
				shipModelList[filestr].replacedFile = "mod\\res\\"..filestr..".png"
				
				local file = io.open("mod\\shipModelList.lua", "w");
				assert(file)
				file:write("shipModelList = {}\n")
				
				for i, v in pairs(shipModelList) do
					file:write("shipModelList["..string.format('%q', i).."] = {}\n")
					file:write("shipModelList["..string.format('%q', i).."].replacedFile = "..string.format('%q', v.replacedFile).."\n")
        end
        
        file:close()
			end
			return shipModelList[filestr].replacedFile
		else
			return nil
		end
	else
		return nil
	end
end