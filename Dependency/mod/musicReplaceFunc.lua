key = "ON" --×Ü¿ª¹Ø 
--key = "OFF"

musicList = 
{
	["move.mp3"] =
	{
		replacedFile = "mod\\res\\Beta-pve.mp3"
	}
}

function musicReplace(music)
	if key == "ON" then
		if musicList[music] then
			return musicList[music].replacedFile
		else
			return nil
		end
	else
		return nil
	end
end