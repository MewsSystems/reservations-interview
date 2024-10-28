import { Grid, Heading, Section } from "@radix-ui/themes";
import { useGetReservations } from "../reservations/api";
import { LoadingCard } from "../components/LoadingCard";
import { ReservationCard } from "./ReservationCard";

export function StaffPage() {
  const { isLoading, isError, data: reservations, error } = useGetReservations();

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint">
        Reservations
      </Heading>

      <Grid columns="1" gap="3">
        {isLoading && <LoadingCard />}
        {isError && <div>{error.message}</div>}
        {reservations?.map((reservation) => (
          <ReservationCard
            roomNumber={reservation.roomNumber}
            start={new Date(reservation.start).toLocaleDateString()}
            end={new Date(reservation.end).toLocaleDateString()}
            guestEmail={reservation.guestEmail}
          />
        ))}

      </Grid>
    </Section>
  );
}
