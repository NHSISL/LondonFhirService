export function isHx(hxNumber: string) {
    return hxNumber.indexOf("-") !== -1
}

export function convertHx(hxNumber: string) {
    const cleanedID = hxNumber.replace(/-/g, '');
    const originalHex = cleanedID.split('').reverse().join('');

    return parseInt(originalHex, 16).toString();
}

export function getPseudo(maybeHxNumber: string) {
    return isHx(maybeHxNumber) ? convertHx(maybeHxNumber) : maybeHxNumber;
}

export function convertToHx(decimalNumber: string) {
    const number = parseInt(decimalNumber, 10);

    if (isNaN(number)) {
        return '';
    }

    const hexString = number.toString(16).toUpperCase();
    const reversedHex = hexString.split('').reverse().join('');
    const formattedHex = reversedHex.padEnd(8, '0').replace(/(.{2})(.{3})(.{3})/, '$1-$2-$3');

    return formattedHex;
}