#!/bin/sh
# Punto de entrada del contenedor de la aplicacion.
#
# Genera, la primera vez, el certificado autofirmado con el que se cifran
# las claves de Data Protection. El certificado vive en un volumen Docker
# compartido por las cuatro replicas: tiene que ser SIEMPRE el mismo en
# todas ellas, porque si cada una usara uno distinto, una cookie emitida por
# app1 no podria descifrarse en app2 y el inicio de sesion se romperia al
# cambiar de replica.
set -e

CERT_DIR="${DATA_PROTECTION_CERT_DIR:-/var/ferreteria/proteccion}"
CERT_FILE="${CERT_DIR}/proteccion.pfx"

mkdir -p "${CERT_DIR}"

if [ ! -f "${CERT_FILE}" ]; then
    echo "[entrypoint] No existe el certificado de Data Protection: se genera uno autofirmado."

    # Se genera en un archivo temporal y se mueve al final: el movimiento es
    # atomico, de modo que otra replica que arranque en ese momento no
    # encuentre un PFX a medio escribir.
    TMP_PFX="${CERT_DIR}/.proteccion.$$.pfx"
    TMP_KEY="${CERT_DIR}/.proteccion.$$.key"
    TMP_CRT="${CERT_DIR}/.proteccion.$$.crt"

    openssl req -x509 -nodes \
        -newkey rsa:2048 \
        -keyout "${TMP_KEY}" \
        -out "${TMP_CRT}" \
        -days 3650 \
        -subj "/CN=FerreteriaJoel-DataProtection" \
        >/dev/null 2>&1

    openssl pkcs12 -export \
        -out "${TMP_PFX}" \
        -inkey "${TMP_KEY}" \
        -in "${TMP_CRT}" \
        -passout pass: \
        >/dev/null 2>&1

    rm -f "${TMP_KEY}" "${TMP_CRT}"

    # ln es atomico: si otra replica ya creo el archivo, falla con "File
    # exists" y no se sobrescribe el certificado existente. Con "mv -n" la
    # operacion no fallaria y las cuatro replicas podrian crearse certificados
    # distintos en el volumen compartido.
    if ln "${TMP_PFX}" "${CERT_FILE}" 2>/dev/null; then
        echo "[entrypoint] Certificado generado: ${CERT_FILE}"
    else
        # Otra replica lo genero primero: se usa el que ya esta.
        rm -f "${TMP_PFX}"
        echo "[entrypoint] Otra replica genero el certificado antes: se reutiliza el existente."
    fi
else
    echo "[entrypoint] Certificado de Data Protection ya disponible: ${CERT_FILE}"
fi

chmod 600 "${CERT_FILE}" 2>/dev/null || true

exec "$@"
