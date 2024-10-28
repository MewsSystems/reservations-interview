import { Grid, Heading, Section } from "@radix-ui/themes";
import { useGetReservationsFromToday } from "../reservations/api";
import { LoadingCard } from "../components/LoadingCard";
import { ReservationCard } from "./ReservationCard";

export function StaffPage() {
  const { isLoading, data: reservations } = useGetReservationsFromToday();

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint">
        Reservations
      </Heading>

      <Grid columns="1" gap="3">
        {isLoading && <LoadingCard />}
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
