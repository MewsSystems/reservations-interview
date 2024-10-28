import { Dialog, Grid, Heading, Section } from "@radix-ui/themes";
import {
  login,
  useCheckCookie,
  useGetReservationsFromToday,
} from "../reservations/api";
import { LoadingCard } from "../components/LoadingCard";
import { ReservationCard } from "./ReservationCard";
import { LoginModal } from "./LoginModal";

export function StaffPage() {
  const { isSuccess: isAuthenticated } = useCheckCookie();
  const { isLoading, data: reservations } = useGetReservationsFromToday();

  const onSubmitPasscode = (passcode: string) => {
    login(passcode);
  };

  return isAuthenticated ? (
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
  ) : (
    <Section size="2" px="2">
      <Dialog.Root open={!isAuthenticated}>
        <LoginModal onSubmit={onSubmitPasscode} />
      </Dialog.Root>
    </Section>
  );
}
