This is a plugin for [Weland](https://github.com/treellama/weland), the Aleph One map editor. It allows exporting an entire Marathon level into JSON format, for ingestion by other tools. (No import of JSON data for now.)

# Usage

Put `MapToJSON.dll` and `Newtonsoft.Json.dll` into your Weland plugins folder, and "Map To JSON" will appear in the Plugins menu.

# Building

Acquire `Newtonsoft.Json.dll` (for .NET 4.0) and put it in the same folder as the Makefile. Run `make`. Your brand-new `MapToJSON.dll` will appear in the `bin` folder.

# Legalese

Map TO JSON is copyright ©2023 Solra Bizna.

Map To JSON is free software: you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation, either version 2 of the License, or (at your option) any later version.

Map To JSON is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU General Public License for more details.

You should have received [a copy of the GNU General Public License](COPYING.txt) along with this program. If not, see https://www.gnu.org/licenses/. 
