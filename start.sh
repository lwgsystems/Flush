#!/usr/bin/env bash

usage()
{
	echo "USAGE: ./start.sh --cert arg [--cert-password arg] --cert-dir arg --data-dir arg
	--cert	the name of your pfx ssl certificate.
        --cert-password 	the password for your ssl certificate, if set.
	--cert-dir		the location of certificates.
	--data-dir		the location of your sqlite files.
	-h | --help		print this mesasage."
}

cert_filename=""
cert_password=""
cert_dir=""
data_dir=""

while [[ "$1" ]]
do
	case $1 in
		--cert)
			shift
			cert_filename=$1
			;;
		--cert-password )
			shift
			cert_password=$1
			;;
		--cert-dir)
			shift
			cert_dir=$1
			;;
		--data-dir)
			shift
			data_dir=$1
			;;
		-h | --help)
			usage
			exit
			;;
		*)
			break
			;;
	esac
	shift
done

if [[ -z "$cert_filename" ]]
then
	echo "cert not set."
	usage
	exit 1
fi

if [[ -z "$cert_dir" ]]
then
	echo "cert_dir not set."
	usage
	exit 1
fi

if [[ -z "$data_dir" ]]
then
	echo "data-dir not set."
	usage
	exit 1
fi

docker run --rm -d -p 443:443 \
	-e ASPNETCORE_URLS="https://+" \
	-e ASPNETCORE_HTTPS_PORT=443 \
	-e ASPNETCORE_Kestrel__Certificates__Default__Password="$cert_password" \
	-e ASPNETCORE_Kestrel__Certificates__Default__Path=/https/"$cert_filename" \
	-v "$cert_dir":/https/ \
	-v "$data_dir":/app/data-sources \
	aspnetcore-5.0-flush-app
