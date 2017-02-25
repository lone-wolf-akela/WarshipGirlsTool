# WarshipGirlsTool
 [![AppVeyor Build Status](https://ci.appveyor.com/api/projects/status/y520hh1qggv42tgf?svg=true)](https://ci.appveyor.com/project/lone-wolf-akela/warshipgirlstool)

A small tool for the game WarshipGirlsR.
My ultimate goal is to write a fully functional PC client for the game. But there's still a long way to go.

为手游《战舰少女R》编写的一个小工具，终极目标是要做一个全功能的战舰少女PC客户端。
* 目前能做到的：
    * 查看远征剩余时间；
    * 远征完成提醒；
    * 查看剩余资源；
    * 港口显示秘书舰立绘；
    * BGM播放；
    * 收远征；
    * 召回远征中的舰队。

##注意
编译生成的文件中的"mod\imageReplaceFunc.lua"中包括使用waifu2x得到高清立绘进行替换的代码。请根据自己的情况修改"key"和"waifu2x_mode"变量以进行相应的控制。
"mod\musicReplaceFunc.lua"中则包含替换音乐所用的代码，同样可根据自己的情况进行修改。
