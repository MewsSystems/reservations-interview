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
import { LoadingCard } from "../components/LoadingCard";
import { useShowInfoToast, useShowSuccessToast } from "../utils/toasts";
import {
  checkStaffSession,
  loginStaff,
  StaffReservation,
  useGetStaffReservations,
} from "./api";

const RESERVATION_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
  sm: "1",
  md: "2",
};

const DimSlot = styled(TextField.Slot)`
  background-color: var(--gray-4);
  margin-right: 8px;
`;

export function StaffPage() {
  const [accessCode, setAccessCode] = useState("");
  const [isAuthorized, setIsAuthorized] = useState(false);
  const [isCheckingAccess, setIsCheckingAccess] = useState(true);
  const [isLoggingIn, setIsLoggingIn] = useState(false);
  const showMissingCodeToast = useShowInfoToast(
    "Enter the shared access code.",
  );
  const showInvalidCodeToast = useShowInfoToast(
    "That access code is incorrect.",
  );
  const showStaffWelcomeToast = useShowSuccessToast("Welcome back.");
  const {
    data: reservations,
    isLoading: isLoadingReservations,
    isError: hasReservationsError,
  } = useGetStaffReservations(isAuthorized);

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
        Upcoming Reservations
      </Heading>

      {isLoadingReservations && (
        <Grid columns={RESERVATION_GRID_COLS} gap="4" px="4" mt="8">
          <LoadingCard />
        </Grid>
      )}

      {!isLoadingReservations && reservations?.length === 0 && (
        <Card size="4" mt="8" mx="4">
          <Text size="4">
            There are no reservations scheduled for today or later.
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

      {reservations && reservations.length > 0 && (
        <Grid columns={RESERVATION_GRID_COLS} gap="4" px="4" mt="8">
          {reservations.map((reservation) => (
            <ReservationSummaryCard
              key={reservation.id}
              reservation={reservation}
            />
          ))}
        </Grid>
      )}
    </Section>
  );
}

function ReservationSummaryCard({
  reservation,
}: {
  reservation: StaffReservation;
}) {
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
      </Flex>
    </Card>
  );
}

function formatDate(dateValue: string) {
  return new Intl.DateTimeFormat(undefined, {
    dateStyle: "medium",
  }).format(new Date(dateValue));
}
