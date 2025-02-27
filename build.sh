#!/bin/bash

DOTNET=/usr/bin/dotnet

# Create build directory
mkdir -p build/ChartConverter

# Create new console project
$DOTNET new console -o build/ChartConverter

# Copy source files
cp "Moonscraper Chart Editor/Assets/Scripts/Game/Charts/IO/Chart/ChartConverterCLI.cs" build/ChartConverter/Program.cs
cp "Moonscraper Chart Editor/Assets/Scripts/Game/Charts/IO/Chart/BatchCLI.cs" build/ChartConverter/BatchCLI.cs
cp "Moonscraper Chart Editor/Assets/Scripts/Game/Charts/IO/Chart/ChartWriter.cs" build/ChartConverter/ChartWriter.cs

# Build the project
cd build/ChartConverter
$DOTNET build -c Release

# Make the output executable
chmod +x bin/Release/net6.0/ChartConverter

# Create symlink in build directory
cd ..
ln -sf ChartConverter/bin/Release/net6.0/ChartConverter chart-converter
