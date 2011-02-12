copy Output\debug\MWClient.exe PublicBuild\MWClient.exe
copy Output\debug\MWShared.dll PublicBuild\MWShared.dll
copy Output\debug\Lidgren.Network.dll PublicBuild\Lidgren.Network.dll
copy Output\debug\Lidgren.Network.Xna.dll PublicBuild\Lidgren.Network.Xna.dll
copy Output\debug\MWServer.exe PublicBuild\MWServer.exe

mkdir PublicBuild\ClientConfigs
mkdir PublicBuild\ServerConfigs
mkdir PublicBuild\Content
xcopy Output\debug\ClientConfigs\* PublicBuild\ClientConfigs\ /S /y
xcopy Output\debug\ServerConfigs\* PublicBuild\ServerConfigs\ /S /y
xcopy Output\debug\Content\* PublicBuild\Content\ /S /y