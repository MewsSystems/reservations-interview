import { useEffect, useState } from "react";
import styled from "styled-components";
import {
  Box,
  Button,
  Card,
  Flex,
  Grid,
  Heading,
  Section,
  Text,
  TextField,
} from "@radix-ui/themes";
import { HTTPError } from "ky";
import { LoadingCard } from "../components/LoadingCard";
import {
  showInfoToast,
  useShowInfoToast,
  useShowSuccessToast,
} from "../utils/toasts";
import {
  checkInReservation,
  checkStaffSession,
  loginStaff,
  StaffReservation,
  useGetStaffReservations,
} from "./api";
import { useQueryClient } from "@tanstack/react-query";

const RESERVATION_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
  sm: "1",
  md: "2",
};

const DimSlot = styled(TextField.Slot)`
  background-color: var(--gray-4);
  margin-right: 8px;
`;

export function StaffPage() {
  const queryClient = useQueryClient();
  const [accessCode, setAccessCode] = useState("");
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [isCheckingAccess, setIsCheckingAccess] = useState(true);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const [isCheckingInReservationId, setIsCheckingInReservationId] =
    useState("");
  const [reservationFilter, setReservationFilter] = useState<
    "upcoming" | "today"
  >("upcoming");
  const showMissingCodeToast = useShowInfoToast(
    "Enter the shared access code.",
  );
  const showInvalidCodeToast = useShowInfoToast(
    "That access code is incorrect.",
  );
  const showStaffWelcomeToast = useShowSuccessToast("Welcome back.");
  const showCheckInSuccessToast = useShowSuccessToast(
    "The guest has been checked in.",
  );
  const {
    data: reservations,
    isLoading: isLoadingReservations,
    isError: hasReservationsError,
  } = useGetStaffReservations(isAuthorized);
  const visibleReservations =
    reservations?.filter((reservation) =>
      reservationFilter === "today"
        ? isReservationForToday(reservation.start)
        : true,
    ) ?? [];

  useEffect(() => {
    let isCancelled = false;

    void checkStaffSession()
      .then((authorized) => {
        if (isCancelled) {
          return;
        }

        setIsAuthorized(authorized);
      })
      .catch(() => {
        if (isCancelled) {
          return;
        }

        setIsAuthorized(false);
      })
      .finally(() => {
        if (!isCancelled) {
          setIsCheckingAccess(false);
        }
      });

    return () => {
      isCancelled = true;
    };
  }, []);

  async function handleLogin() {
    if (!accessCode.trim()) {
      showMissingCodeToast();
      return;
    }

    setIsLoggingIn(true);

    try {
      const authorized = await loginStaff(accessCode.trim());
      setIsAuthorized(authorized);

      if (!authorized) {
        showInvalidCodeToast();
        return;
      }

      setAccessCode("");
      showStaffWelcomeToast();
    } finally {
      setIsLoggingIn(false);
      setIsCheckingAccess(false);
    }
  }

  async function handleCheckIn(reservationId: string, guestEmail: string) {
    setIsCheckingInReservationId(reservationId);

    try {
      await checkInReservation(reservationId, guestEmail);
      await queryClient.invalidateQueries({
        queryKey: ["staff", "reservations"],
      });
      showCheckInSuccessToast();
    } catch (error) {
      if (error instanceof HTTPError) {
        const errorMessage = await error.response.text();
        if (errorMessage) {
          showInfoToast(errorMessage);
          return;
        }
      }

      showInfoToast("We could not check in the guest right now.");
    } finally {
      setIsCheckingInReservationId("");
    }
  }

  if (isCheckingAccess) {
    return (
      <Section size="2" px="2">
        <Heading size="8" as="h1" color="mint">
          Staff
        </Heading>
        <Grid columns={RESERVATION_GRID_COLS} gap="4" px="4" mt="8">
          <LoadingCard />
        </Grid>
      </Section>
    );
  }

  if (!isAuthorized) {
    return (
      <Section size="2" px="2">
        <Heading size="8" as="h1" color="mint">
          Staff Login
        </Heading>
        <Card size="4" mt="8" mx="4">
          <Flex direction="column" gap="4">
            <Text size="3">
              Enter the shared access code to review current and upcoming
              reservations.
            </Text>
            <TextField.Root
              placeholder="... shared access code ..."
              value={accessCode}
              onChange={(evt) => setAccessCode(evt.target.value)}
              type="password"
              size="3"
              disabled={isLoggingIn}
            >
              <DimSlot side="left" prefix="staff">
                Access
              </DimSlot>
            </TextField.Root>
            <Flex justify="end">
              <Button
                size="3"
                color="mint"
                onClick={handleLogin}
                loading={isLoggingIn}
                disabled={isLoggingIn}
              >
                Log In
              </Button>
            </Flex>
          </Flex>
        </Card>
      </Section>
    );
  }

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint">
        {reservationFilter === "today"
          ? "Today's Reservations"
          : "Upcoming Reservations"}
      </Heading>

      <Flex gap="3" px="4" mt="6">
        <Button
          size="3"
          color="mint"
          variant={reservationFilter === "upcoming" ? "solid" : "soft"}
          onClick={() => setReservationFilter("upcoming")}
        >
          All Upcoming
        </Button>
        <Button
          size="3"
          color="mint"
          variant={reservationFilter === "today" ? "solid" : "soft"}
          onClick={() => setReservationFilter("today")}
        >
          Today
        </Button>
      </Flex>

      {isLoadingReservations && (
        <Grid columns={RESERVATION_GRID_COLS} gap="4" px="4" mt="8">
          <LoadingCard />
        </Grid>
      )}

      {!isLoadingReservations &&
        !hasReservationsError &&
        visibleReservations.length === 0 && (
          <Card size="4" mt="8" mx="4">
            <Text size="4">
              {reservationFilter === "today"
                ? "There are no reservations starting today."
                : "There are no reservations scheduled for today or later."}
            </Text>
          </Card>
        )}

      {hasReservationsError && (
        <Card size="4" mt="8" mx="4">
          <Text size="4">
            We could not load reservations right now. Please try again.
          </Text>
        </Card>
      )}

      {visibleReservations.length > 0 && (
        <Grid columns={RESERVATION_GRID_COLS} gap="4" px="4" mt="8">
          {visibleReservations.map((reservation) => (
            <ReservationSummaryCard
              key={reservation.id}
              reservation={reservation}
              isCheckingIn={isCheckingInReservationId === reservation.id}
              onCheckIn={handleCheckIn}
            />
          ))}
        </Grid>
      )}
    </Section>
  );
}

function ReservationSummaryCard({
  reservation,
  isCheckingIn,
  onCheckIn,
}: {
  reservation: StaffReservation;
  isCheckingIn: boolean;
  onCheckIn: (reservationId: string, guestEmail: string) => Promise<void>;
}) {
  const [confirmationEmail, setConfirmationEmail] = useState("");
  const showMissingConfirmationToast = useShowInfoToast(
    "Enter the guest email to confirm check in.",
  );
  const canCheckIn =
    isReservationForToday(reservation.start) && !reservation.checkedIn;

  async function handleCheckIn() {
    if (!confirmationEmail.trim()) {
      showMissingConfirmationToast();
      return;
    }

    await onCheckIn(reservation.id, confirmationEmail.trim());
    setConfirmationEmail("");
  }

  return (
    <Card size="4">
      <Flex direction="column" gap="3">
        <Box>
          <Text size="2" color="gray">
            Room
          </Text>
          <Heading size="6">#{reservation.roomNumber}</Heading>
        </Box>
        <Box>
          <Text size="2" color="gray">
            Guest
          </Text>
          <Text size="4">{reservation.guestEmail}</Text>
        </Box>
        <Box>
          <Text size="2" color="gray">
            Stay
          </Text>
          <Text size="3">
            {formatDate(reservation.start)} to {formatDate(reservation.end)}
          </Text>
        </Box>
        <Box>
          <Text size="2" color="gray">
            Status
          </Text>
          <Text size="3">
            {reservation.checkedIn ? "Checked in" : "Not checked in"}
          </Text>
        </Box>

        {canCheckIn && (
          <Flex direction="column" gap="3">
            <Text size="2" color="gray">
              Confirm guest email to check in today&apos;s arrival.
            </Text>
            <TextField.Root
              placeholder="... guest@email.com ..."
              value={confirmationEmail}
              onChange={(evt) => setConfirmationEmail(evt.target.value)}
              type="email"
              size="3"
              disabled={isCheckingIn}
            >
              <DimSlot side="left" prefix="guest">
                Email
              </DimSlot>
            </TextField.Root>
            <Flex justify="end">
              <Button
                size="3"
                color="mint"
                onClick={handleCheckIn}
                loading={isCheckingIn}
                disabled={isCheckingIn}
              >
                Check In
              </Button>
            </Flex>
          </Flex>
        )}
      </Flex>
    </Card>
  );
}

function formatDate(dateValue: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
  }).format(new Date(dateValue));
}

function isReservationForToday(dateValue: string) {
  const reservationDate = new Date(dateValue);
  const today = new Date();

  return (
    reservationDate.getFullYear() === today.getFullYear() &&
    reservationDate.getMonth() === today.getMonth() &&
    reservationDate.getDate() === today.getDate()
  );
}
