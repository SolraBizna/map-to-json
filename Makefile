CC=mcs
WELAND=../weland/Weland.exe

all: bin/MapToJSON.dll

bin/MapToJSON.dll: src/MapToJSON.cs
	@mkdir -p bin
	$(CC) -r:../weland/Weland.exe -r:Newtonsoft.Json.dll -pkg:gtk-sharp-2.0 -target:library -out:$@ $<

clean:
	rm -rf bin
