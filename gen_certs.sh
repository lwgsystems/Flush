#!/usr/bin/env bash

usage()
{
	echo "USAGE: ./gen_certs [--clean]
	--clean remove crt and key files after production of a pfx file"
}

appsettings="Flush/appsettings.json"
clean=""

while [[ "$1" ]]
do
	case $1 in
		--clean)
			clean="true"
			;;
		-h | --help)
			usage
			exit
			;;
	esac
	shift
done

for cert in flush identity jwt
do
	key="$cert".key
	crt="$cert".crt
	pfx="$cert".pfx
	searchstring="$cert"_thumbprint

	# generate a private and public key
	openssl req -x509 -nodes -days 365 -newkey rsa:2048 -keyout "$key" -out "$crt" \
		-subj "/C=UK/ST=County/L=City/O=Flush/OU=FlushDevelopment/CN=flush-"$cert""

	# get the thumbprint
	thumbprint=$(openssl x509 -in "$crt" -noout -fingerprint | awk -F= '{ gsub(":",""); print $2 }')

	# preconfigure a sed-safe substitution
	sedscript="s/""$searchstring""/""$thumbprint""/"

	# drop it into appsettings
	sed -E -i'.bak' -e $sedscript $appsettings

	# export the key and cert as a pfx file
	openssl pkcs12 -export -out "$pfx" -inkey "$key" -in "$crt" -passout pass:
done

if [[ ! -z "$clean" ]]
then
	rm *.crt *.key
fi

