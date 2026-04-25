import { useState } from "react";
import { useShowErrorToast, useShowSuccessToast } from "../utils/toasts";
import { Grid, Heading, Section, Dialog } from "@radix-ui/themes";
import { ReservationCard } from "./ReservationCard";
import { bookRoom, NewReservation, useGetRooms } from "./api";
import { LoadingCard } from "../components/LoadingCard";
import { BookingDetailsModal } from "./BookingDetailsModal";

const RESPONSIVE_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
  sm: "1",
  md: "2",
  lg: "4",
};

export function ReservationPage() {
  const { isLoading, data: rooms } = useGetRooms();
  const [selectedRoomNumber, setSelectedRoomNumber] = useState("");

  const formattedRoomNumber = String(selectedRoomNumber).padStart(3, "0");

  const showToast = useShowSuccessToast("We have received your booking!");

  const showErrorToast = useShowErrorToast();

  function onClose() {
    setSelectedRoomNumber("");
  }

async function onSubmit(booking: NewReservation) {
  try {
    await bookRoom(booking);
    onClose();
    showToast();
  } catch (err: any) {
    let errorMessage = "An unexpected error occurred.";

    if (err.name === "HTTPError") {
      try {
        const data = await err.response.json();
        const errors = Array.isArray(data.errors) 
          ? data.errors 
          : Object.values(data.errors || {}).flat();
        
        errorMessage = errors.length > 0 ? errors.join("\n") : "Room may be unavailable.";
      } catch {
        errorMessage = (await err.response.text()) || "Unknown error.";
      }
    }

    showErrorToast(`Booking failed: ${errorMessage}`);
  }
}

  const createClickHandler = (roomNumber: string) => () => {
    setSelectedRoomNumber(roomNumber);
  };

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint">
        Rooms
      </Heading>

      <Grid columns={RESPONSIVE_GRID_COLS} gap="4" px="4" mt="8">
        <Dialog.Root>
          {isLoading && <LoadingCard />}
          {rooms?.map((room) => (
            <ReservationCard
              key={room.Number}
              imgSrc="/bed.png"
              roomNumber={room.Number}
              onClick={createClickHandler(room.Number)}
            />
          ))}

          <BookingDetailsModal
            roomNumber={formattedRoomNumber}
            onSubmit={onSubmit}
          />
        </Dialog.Root>
      </Grid>
    </Section>
  );
}
