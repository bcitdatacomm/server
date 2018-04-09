#!/bin/bash

echo "Removing any old versions of the server..."
rm *.exe

echo "Compiling server..."
mcs -unsafe -out:server.exe *.cs

echo "Running server..."
mono server.exe
