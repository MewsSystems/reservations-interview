import { useState } from "react";
import { useShowSuccessToast } from "../utils/toasts";
import { Grid, Heading, Section, Dialog } from "@radix-ui/themes";
import { ReservationCard } from "../components/ReservationCard";
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

  function onClose() {
    setSelectedRoomNumber("");
  }

  function onSubmit(booking: NewReservation) {
    console.log(booking)
    bookRoom(booking).then(onClose).then(showToast);
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
              key={room.number}
              imgSrc="/bed.png"
              title={`Room #${room.number}`}
              onClick={createClickHandler(room.number)}
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
