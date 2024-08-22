import { ISO8601String } from "../utils/datetime";
import { useState } from "react";
import { useShowSuccessToast } from "../utils/toasts";
import { Grid, Heading, Section, Dialog } from "@radix-ui/themes";
import { ReservationCard } from "./ReservationCard";
import { bookRoom, useGetRooms } from "./api";
import { LoadingCard } from "../components/LoadingCard";
import { BookingDetailsModal } from "./BookingDetailsModal";

export interface NewReservation {
  RoomNumber: string;
  GuestEmail: string;
  Start: ISO8601String;
  End: ISO8601String;
}

/** The type the API returns */
export interface ReservationAPI {
  Id: string;
  RoomNumber: number;
  GuestEmail: string;
  Start: string;
  End: string;
}

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
    return bookRoom(booking).then(onClose).then(showToast);
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
              roomNumber={room.number}
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
