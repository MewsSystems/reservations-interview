const availabilityCache: { [key: string]: boolean } = {};

export async function checkRoomAvailability(roomNumber: string, startDate: string, endDate: string) {
  const cacheKey = `${roomNumber}-${startDate}-${endDate}`;

  if (availabilityCache[cacheKey] !== undefined) {
    return availabilityCache[cacheKey];
  }

  try {
    const response = await fetch(`/api/room/checkRoomAvailability?roomNumber=${roomNumber}&startDate=${startDate}&endDate=${endDate}`);
    
    if (!response.ok) {
      throw new Error(`Failed to fetch. Status: ${response.status}`);
    }

    const data = await response.json();

    if (data && typeof data.available === 'boolean') {

      availabilityCache[cacheKey] = data.available;

      if (!data.available) {
        alert(
          `The room #${roomNumber} is already booked for the selected dates (${startDate} to ${endDate}). Please choose different dates.`
        );
      }

      return data.available;
    } else {
      throw new Error('Invalid response format');
    }
  } catch (error) {
    console.error('Error checking room availability:', error);
    return false; 
  }
}