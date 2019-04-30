cd ../publish/
zipName=$(ls *.zip)
unzip $zipName
chmod +x bin/supply
chmod +x bin/detect
chmod +x bin/finalize
chmod +x bin/release
rm $zipName
zip -r $zipName .
rm -r bin
