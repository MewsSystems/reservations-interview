

export function validateRoomNumber(roomNumber: string): string[] {
    const errors: string[] = [];
    if (!/^\d{3}$/.test(roomNumber)) {
        errors.push('Room number must be exactly 3 digits.');
        return errors;
    }
    const floorNumber = parseInt(roomNumber[0], 10);
    const doorNumber = parseInt(roomNumber.slice(1), 10);
    if (floorNumber < 0 || floorNumber > 9) {
        errors.push('Floor number must be between 0 and 9.');
    }
    if (doorNumber === 0) {
        errors.push('Door number cannot be "00".');
    }

    return errors;
}
