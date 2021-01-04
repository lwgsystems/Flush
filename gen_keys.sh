#!/usr/bin/env bash

usage ()
{
    echo "USAGE: ./gen_keys [-h]
    -h | --help     display this message"
}

appsettings="Flush/appsettings.json"

while [[ "$1" ]]
do
    case $1 in
        -h | --help)
            usage
            exit
            ;;
    esac
    shift
done

for id in flush identity jwt
do
    searchstring="$id"_key
    key=$(hexdump -n 256 -e '"%08X"' /dev/random)
    sed -E -i'.bak' -e "s/""$searchstring""/""$key""/" $appsettings
done
