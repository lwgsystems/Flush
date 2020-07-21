#!/usr/bin/env bash

./certificate-tool add --file flush.pfx --password ""
./certificate-tool add --file identity.pfx --password ""
./certificate-tool add --file jwt.pfx --password ""

dotnet Flush.dll
