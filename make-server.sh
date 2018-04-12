#!/bin/sh

cd src

echo "----Making server networking library----"

make clean && make server
cp ./libNetwork.so ../

echo "----Finished making server networking library----"

cd ..

echo "----Making Game Server----"

mkbundle -o server server.exe --deps -L /usr/lib/mono/4.5/

echo "----Finished making Game Server----"


