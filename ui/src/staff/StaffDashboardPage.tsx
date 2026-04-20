import { useEffect } from "react";
import { useNavigate } from "@tanstack/react-router";
import { Badge, Flex, Heading, Section, Table, Text } from "@radix-ui/themes";
import { useGetUpcomingReservations } from "../reservations/api";
import { useStaffAuthCheck } from "./api";
import { LoadingCard } from "../components/LoadingCard";

function formatDate(iso: string) {
  return new Date(iso).toLocaleDateString(undefined, {
    year: "numeric",
    month: "short",
    day: "numeric",
  });
}

function isToday(iso: string) {
  return new Date(iso).toDateString() === new Date().toDateString();
}

export function StaffDashboardPage() {
  const navigate = useNavigate();
  const { data: isAuthorized, isLoading: authLoading } = useStaffAuthCheck();
  const { data: reservations, isLoading: reservationsLoading } = useGetUpcomingReservations({ enabled: isAuthorized === true });

  useEffect(() => {
    if (!authLoading && isAuthorized === false) {
      navigate({ to: "/" });
    }
  }, [authLoading, isAuthorized, navigate]);

  if (authLoading) return <LoadingCard />;

  return (
    <Section size="2" px="2">
      <Heading size="8" as="h1" color="mint" mb="6">
        Upcoming Reservations
      </Heading>

      {reservationsLoading && <LoadingCard />}

      {!reservationsLoading && reservations?.length === 0 && (
        <Text color="gray">No upcoming reservations.</Text>
      )}

      {reservations && reservations.length > 0 && (
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Guest Email</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Check-in</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Check-out</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {reservations.map((r) => (
              <Table.Row key={r.id}>
                <Table.Cell>
                  <Text weight="medium">{r.roomNumber}</Text>
                </Table.Cell>
                <Table.Cell>{r.guestEmail}</Table.Cell>
                <Table.Cell>
                  <Flex align="center" gap="2">
                    {formatDate(r.start)}
                    {isToday(r.start) && (
                      <Badge color="green" size="1">Today</Badge>
                    )}
                  </Flex>
                </Table.Cell>
                <Table.Cell>{formatDate(r.end)}</Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table.Root>
      )}
    </Section>
  );
}
