$GamePath="C:\Program Files (x86)\Steam\steamapps\common\worldbox\worldbox_Data\StreamingAssets\mods"
$BuildPath="bin\Debug\net48"

Copy-Item -Path "$BuildPath\NeoModLoader.dll" -Destination "$GamePath\NeoModLoader.dll" -Force
Copy-Item -Path "$BuildPath\NeoModLoader.pdb" -Destination "$GamePath\NeoModLoader.pdb" -Force
Copy-Item -Path "$BuildPath\NeoModLoader.xml" -Destination "$GamePath\NeoModLoader.xml" -Force
#Remove-Item -Path "$GamePath\NML\mod_compile_records.json"