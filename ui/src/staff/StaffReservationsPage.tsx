import { useState } from "react";
import { Badge, Box, Button, Card, Flex, Heading, Section, Select, Table, Text, TextField } from "@radix-ui/themes";
import { useNavigate } from "@tanstack/react-router";
import { useQueryClient } from "@tanstack/react-query";
import { useAuth } from "../utils/auth";
import { checkInReservation, checkOutReservation, RoomStateLabels, ReservationFilters, StaffReservation, useGetStaffReservations } from "./api";
import { Room, useGetRooms } from "../reservations/api";
import { parseApiError } from "../reservations/api";
import { useShowErrorToast, useShowSuccessToast } from "../utils/toasts";
import { LoadingCard } from "../components/LoadingCard";

function todayStr() {
  return new Date().toISOString().split("T")[0];
}

export function StaffReservationsPage() {
  const { token, isAuthenticated } = useAuth();
  const navigate = useNavigate();

  const [filters, setFilters] = useState<ReservationFilters>({
    from: todayStr(),
  });

  const { data: reservations, isLoading } = useGetStaffReservations(token, filters);
  const { data: rooms } = useGetRooms();
  const queryClient = useQueryClient();
  const showError = useShowErrorToast();
  const showCheckInSuccess = useShowSuccessToast("Guest checked in successfully!");
  const showCheckOutSuccess = useShowSuccessToast("Guest checked out successfully!");
  const [checkingIn, setCheckingIn] = useState<string | null>(null);
  const [checkingOut, setCheckingOut] = useState<string | null>(null);

  const roomsByNumber = new Map(rooms?.map((r) => [r.number, r]) ?? []);

  function coversToday(start: string, end: string) {
    const today = new Date().toDateString();
    return new Date(start) <= new Date(today) && new Date(end) > new Date(today);
  }

  function isRoomDirty(roomNumber: string) {
    const room = roomsByNumber.get(roomNumber);
    return room?.state === 2;
  }

  async function handleCheckIn(r: StaffReservation) {
    setCheckingIn(r.id);
    try {
      await checkInReservation(token!, r.id, r.guestEmail);
      showCheckInSuccess();
      queryClient.invalidateQueries({ queryKey: ["staff-reservations"] });
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    } catch (error) {
      const errors = await parseApiError(error);
      showError(errors);
    } finally {
      setCheckingIn(null);
    }
  }

  async function handleCheckOut(r: StaffReservation) {
    setCheckingOut(r.id);
    try {
      await checkOutReservation(token!, r.id, r.guestEmail);
      showCheckOutSuccess();
      queryClient.invalidateQueries({ queryKey: ["staff-reservations"] });
      queryClient.invalidateQueries({ queryKey: ["rooms"] });
    } catch (error) {
      const errors = await parseApiError(error);
      showError(errors);
    } finally {
      setCheckingOut(null);
    }
  }

  if (!isAuthenticated) {
    navigate({ to: "/" });
    return null;
  }

  function updateFilter(key: keyof ReservationFilters, value: string) {
    setFilters((prev) => ({ ...prev, [key]: value || undefined }));
  }

  return (
    <Section size="1" px="2">
      <Heading size="8" as="h1" color="mint" mb="6">
        Reservations
      </Heading>

      <Card size="2" mb="4" variant="surface">
        <Flex gap="4" wrap="wrap" align="end">
          <Box style={{ flex: 1, minWidth: 150 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">From</Text>
            <TextField.Root
              type="date"
              value={filters.from ?? ""}
              onChange={(e) => updateFilter("from", e.target.value)}
              size="2"
            />
          </Box>
          <Box style={{ flex: 1, minWidth: 150 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">To</Text>
            <TextField.Root
              type="date"
              value={filters.to ?? ""}
              onChange={(e) => updateFilter("to", e.target.value)}
              size="2"
            />
          </Box>
          <Box style={{ flex: 1, minWidth: 120 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">Room</Text>
            <Select.Root
              value={filters.roomNumber ?? "all"}
              onValueChange={(v) => updateFilter("roomNumber", v === "all" ? "" : v)}
              size="2"
            >
              <Select.Trigger style={{ width: "100%" }} />
              <Select.Content>
                <Select.Item value="all">All rooms</Select.Item>
                {rooms?.map((room) => (
                  <Select.Item key={room.number} value={room.number}>
                    #{room.number}
                  </Select.Item>
                ))}
              </Select.Content>
            </Select.Root>
          </Box>
          <Box style={{ flex: 2, minWidth: 200 }}>
            <Text as="label" size="2" weight="medium" color="gray" mb="1">Guest Email</Text>
            <TextField.Root
              placeholder="e.g. guest@email.com"
              value={filters.guestEmail ?? ""}
              onChange={(e) => updateFilter("guestEmail", e.target.value)}
              size="2"
            />
          </Box>
        </Flex>
      </Card>

      {isLoading && <LoadingCard />}

      {reservations && reservations.length === 0 && (
        <Box py="4">No reservations found.</Box>
      )}

      {reservations && reservations.length > 0 && (
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Guest Email</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Check-in</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Check-out</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Reservation Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Room Status</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell></Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {reservations.map((r) => (
              <Table.Row key={r.id}>
                <Table.Cell>#{r.roomNumber}</Table.Cell>
                <Table.Cell>{r.guestEmail}</Table.Cell>
                <Table.Cell>{new Date(r.start).toLocaleDateString()}</Table.Cell>
                <Table.Cell>{new Date(r.end).toLocaleDateString()}</Table.Cell>
                <Table.Cell>
                  {r.checkedOut
                    ? <Badge color="blue">Checked Out {r.checkedOutAt && <Text weight="light" size="1">{new Date(r.checkedOutAt).toLocaleDateString()}</Text>}</Badge>
                    : r.checkedIn
                      ? <Badge color="green">Checked In</Badge>
                      : <Badge color="gray">Pending</Badge>}
                </Table.Cell>
                <Table.Cell>
                  {(() => {
                    const room = roomsByNumber.get(r.roomNumber);
                    const label = room ? RoomStateLabels[room.state] ?? "Unknown" : "—";
                    const color = room?.state === 0 ? "green" : room?.state === 1 ? "orange" : room?.state === 2 ? "yellow" : "gray";
                    return <Badge color={color}>{label}</Badge>;
                  })()}
                </Table.Cell>
                <Table.Cell>
                  {!r.checkedIn && coversToday(r.start, r.end) && (
                    <Button
                      size="1"
                      color="mint"
                      disabled={checkingIn === r.id || isRoomDirty(r.roomNumber)}
                      onClick={() => handleCheckIn(r)}
                      title={isRoomDirty(r.roomNumber) ? "Room must be cleaned first" : undefined}
                    >
                      {checkingIn === r.id ? "..." : "Check In"}
                    </Button>
                  )}
                  {r.checkedIn && !r.checkedOut && (
                    <Button
                      size="1"
                      color="orange"
                      disabled={checkingOut === r.id}
                      onClick={() => handleCheckOut(r)}
                    >
                      {checkingOut === r.id ? "..." : "Check Out"}
                    </Button>
                  )}
                </Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table.Root>
      )}
    </Section>
  );
}
