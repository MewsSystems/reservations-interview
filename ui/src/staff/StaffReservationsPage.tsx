import { useState } from "react";
import {
  Badge,
  Box,
  Button,
  Dialog,
  Flex,
  Heading,
  Section,
  Separator,
  Switch,
  Table,
  Text,
  TextField,
} from "@radix-ui/themes";
import { useGetStaffReservations, useCheckIn, useGetRoomsState, useSetRoomState, RoomState, StaffReservation, StaffRoom } from "./api";
import { LoadingCard } from "../components/LoadingCard";
import { useSortableTable } from "../utils/useSortableTable";

function RoomStateDialog({
  room,
  targetState,
  onClose,
}: {
  room: StaffRoom;
  targetState: RoomState;
  onClose: () => void;
}) {
  const setRoomState = useSetRoomState();
  const label = targetState === RoomState.Ready ? "Clean" : "Dirty";

  async function handleConfirm() {
    await setRoomState.mutateAsync({ roomNumber: room.number, state: targetState });
    onClose();
  }

  return (
    <Dialog.Content size="3">
      <Dialog.Title>Mark Room #{room.number} as {label}</Dialog.Title>
      <Dialog.Description mb="4">
        Are you sure you want to mark room #{room.number} as <strong>{label}</strong>?
      </Dialog.Description>
      <Flex gap="3" justify="end">
        <Dialog.Close>
          <Button variant="soft" color="gray" onClick={onClose}>
            Cancel
          </Button>
        </Dialog.Close>
        <Button
          color={targetState === RoomState.Ready ? "green" : "red"}
          onClick={handleConfirm}
          disabled={setRoomState.isPending}
        >
          {setRoomState.isPending ? "Saving..." : `Mark ${label}`}
        </Button>
      </Flex>
    </Dialog.Content>
  );
}

function CheckInDialog({
  reservation,
  onClose,
}: {
  reservation: StaffReservation;
  onClose: () => void;
}) {
  const [email, setEmail] = useState("");
  const [error, setError] = useState("");
  const checkIn = useCheckIn();

  async function handleConfirm() {
    setError("");
    try {
      await checkIn.mutateAsync({ reservationId: reservation.id, guestEmail: email });
      onClose();
    } catch (_e: any) {
      const message = await _e?.response?.text().catch(() => null);
      setError(message?.replace(/^"|"$/g, "") || "Check-in failed. Please try again.");
    }
  }

  return (
    <Dialog.Content size="3">
      <Dialog.Title>Check In - Room #{reservation.roomNumber}</Dialog.Title>
      <Dialog.Description mb="4">
        Enter the guest email address to confirm check-in.
      </Dialog.Description>
      <Flex direction="column" gap="3">
        <Box>
          <Text as="label" size="2" weight="medium" htmlFor="confirm-email">
            Guest Email
          </Text>
          <TextField.Root
            id="confirm-email"
            type="email"
            placeholder={reservation.guestEmail}
            value={email}
            onChange={(e) => setEmail(e.target.value)}
            mt="1"
          />
        </Box>
        {error && (
          <Text color="red" size="2">
            {error}
          </Text>
        )}
        <Flex gap="3" justify="end">
          <Dialog.Close>
            <Button variant="soft" color="gray" onClick={onClose}>
              Cancel
            </Button>
          </Dialog.Close>
          <Button onClick={handleConfirm} disabled={!email || checkIn.isPending}>
            {checkIn.isPending ? "Checking in..." : "Confirm Check In"}
          </Button>
        </Flex>
      </Flex>
    </Dialog.Content>
  );
}

export function StaffReservationsPage() {
  const { isLoading, data: reservations, isError } = useGetStaffReservations();
  const [todayOnly, setTodayOnly] = useState(false);
  const [selectedReservation, setSelectedReservation] = useState<StaffReservation | null>(null);

  const { data: rooms } = useGetRoomsState();
  const [roomStateDialog, setRoomStateDialog] = useState<{ room: StaffRoom; targetState: RoomState } | null>(null);

  const today = new Date().toLocaleDateString();
  const isToday = (dateStr: string) => new Date(dateStr).toLocaleDateString() === today;

  const filtered = reservations?.filter((r) => todayOnly ? isToday(r.start) : true);

  type SortKey = "roomNumber" | "guestEmail" | "start" | "end" | "status";

  // Derive a stable string for the "status" pseudo-column so the hook can sort it.
  const getStatus = (r: StaffReservation): string =>
    r.checkedOut ? "Checked Out" : r.checkedIn ? "Checked In" : "Upcoming";

  const { sorted, sortState, toggleSort } = useSortableTable<
    StaffReservation & { status: string },
    SortKey
  >(
    filtered?.map((r) => ({ ...r, status: getStatus(r) })),
    "start",
    "asc"
  );

  // Arrow indicator for active sort column
  const arrow = (key: SortKey) =>
    sortState.key !== key ? " ↕" : sortState.direction === "asc" ? " ↑" : " ↓";

  // Clickable header cell
  const SortHeader = ({ col, label }: { col: SortKey; label: string }) => (
    <Table.ColumnHeaderCell
      onClick={() => toggleSort(col)}
      style={{ cursor: "pointer", userSelect: "none", whiteSpace: "nowrap" }}
    >
      {label}{arrow(col)}
    </Table.ColumnHeaderCell>
  );

  return (
    <Section size="2" px="4">
      <Flex align="center" justify="between" mb="6">
        <Heading size="8" as="h1" color="mint">
          Upcoming Reservations
        </Heading>
        <Flex align="center" gap="2">
          <Text size="2">Today only</Text>
          <Switch checked={todayOnly} onCheckedChange={setTodayOnly} />
        </Flex>
      </Flex>

      {isLoading && <LoadingCard />}

      {isError && (
        <Text color="red">Failed to load reservations. Please log in again.</Text>
      )}

      {filtered && filtered.length === 0 && (
        <Text color="gray">No reservations found.</Text>
      )}

      {filtered && filtered.length > 0 && (
        <Dialog.Root
          open={!!selectedReservation}
          onOpenChange={(open) => { if (!open) setSelectedReservation(null); }}
        >
          <Table.Root variant="surface">
            <Table.Header>
              <Table.Row>
                <SortHeader col="roomNumber" label="Room" />
                <SortHeader col="guestEmail" label="Guest Email" />
                <SortHeader col="start" label="Start" />
                <SortHeader col="end" label="End" />
                <SortHeader col="status" label="Status" />
                <Table.ColumnHeaderCell />
              </Table.Row>
            </Table.Header>
            <Table.Body>
              {sorted.map((r) => (
                <Table.Row key={r.id}>
                  <Table.Cell>#{r.roomNumber}</Table.Cell>
                  <Table.Cell>{r.guestEmail}</Table.Cell>
                  <Table.Cell>{new Date(r.start).toLocaleDateString()}</Table.Cell>
                  <Table.Cell>{new Date(r.end).toLocaleDateString()}</Table.Cell>
                  <Table.Cell>
                    <Flex gap="1">
                      {r.checkedIn && <Badge color="green">Checked In</Badge>}
                      {r.checkedOut && <Badge color="gray">Checked Out</Badge>}
                      {!r.checkedIn && !r.checkedOut && <Badge color="blue">Upcoming</Badge>}
                    </Flex>
                  </Table.Cell>
                  <Table.Cell>
                    {!r.checkedIn && isToday(r.start) && (
                      <Button size="1" onClick={() => setSelectedReservation(r)}>
                        Check In
                      </Button>
                    )}
                  </Table.Cell>
                </Table.Row>
              ))}
            </Table.Body>
          </Table.Root>

          {selectedReservation && (
            <CheckInDialog
              reservation={selectedReservation}
              onClose={() => setSelectedReservation(null)}
            />
          )}
        </Dialog.Root>
      )}

      <Separator my="6" size="4" />

      <Heading size="6" as="h2" color="mint" mb="4">
        Housekeeping
      </Heading>

      {rooms && rooms.length > 0 && (
        <Dialog.Root
          open={!!roomStateDialog}
          onOpenChange={(open) => { if (!open) setRoomStateDialog(null); }}
        >
        <Table.Root variant="surface">
          <Table.Header>
            <Table.Row>
              <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>State</Table.ColumnHeaderCell>
              <Table.ColumnHeaderCell>Actions</Table.ColumnHeaderCell>
            </Table.Row>
          </Table.Header>
          <Table.Body>
            {rooms.map((room) => (
              <Table.Row key={room.number}>
                <Table.Cell>#{room.number}</Table.Cell>
                <Table.Cell>
                  {room.state === RoomState.Ready && <Badge color="green">Clean</Badge>}
                  {room.state === RoomState.Dirty && <Badge color="red">Dirty</Badge>}
                  {room.state === RoomState.Occupied && <Badge color="blue">Occupied</Badge>}
                </Table.Cell>
                <Table.Cell>
                  <Flex gap="2">
                    {room.state !== RoomState.Ready && (
                      <Button
                        size="1"
                        variant="soft"
                        color="green"
                        onClick={() => setRoomStateDialog({ room, targetState: RoomState.Ready })}
                      >
                        Mark Clean
                      </Button>
                    )}
                    {room.state !== RoomState.Dirty && (
                      <Button
                        size="1"
                        variant="soft"
                        color="red"
                        onClick={() => setRoomStateDialog({ room, targetState: RoomState.Dirty })}
                      >
                        Mark Dirty
                      </Button>
                    )}
                  </Flex>
                </Table.Cell>
              </Table.Row>
            ))}
          </Table.Body>
        </Table.Root>

        {roomStateDialog && (
          <RoomStateDialog
            room={roomStateDialog.room}
            targetState={roomStateDialog.targetState}
            onClose={() => setRoomStateDialog(null)}
          />
        )}
      </Dialog.Root>
      )}
    </Section>
  );
}
