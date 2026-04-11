import { type FormEvent, useState } from "react";
import {
  Button,
  Card,
  Flex,
  Heading,
  Section,
  Table,
  Text,
  TextField,
} from "@radix-ui/themes";
import { HTTPError } from "ky";
import { getStaffReservations, loginAsStaff, type StaffReservation } from "./api";

export function StaffPage() {
  const [accessCode, setAccessCode] = useState("");
  const [reservations, setReservations] = useState<StaffReservation[] | null>(null);
  const [error, setError] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const isLoggedIn = reservations !== null;

  async function onSubmit(event: FormEvent<HTMLFormElement>) {
    event.preventDefault();
    setIsLoading(true);
    setError("");

    try {
      await loginAsStaff(accessCode);
      setReservations(await getStaffReservations());
    } catch (err) {
      if (err instanceof HTTPError && err.response.status === 401) {
        setError("Invalid access code.");
      } else {
        setError("Failed to load reservations.");
      }
      setReservations(null);
    } finally {
      setIsLoading(false);
    }
  }

  return (
    <Section size="2" px="2">
      <Flex direction="column" gap="5" px="4" py="4">
        <div>
          <Heading size="8" as="h1" color="mint">
            Staff reservations
          </Heading>
          <Text size="3" color="gray">
            Sign in with the shared access code to view today and future reservations.
          </Text>
        </div>

        {isLoggedIn ? null : (
          <Card size="3">
            <form onSubmit={onSubmit}>
              <Flex direction="column" gap="3">
                <TextField.Root
                  type="password"
                  value={accessCode}
                  onChange={(event) => setAccessCode(event.target.value)}
                  placeholder="Shared access code"
                />
                <Button type="submit" disabled={isLoading || !accessCode.trim()}>
                  {isLoading ? "Loading..." : "Sign in"}
                </Button>
              </Flex>
            </form>
          </Card>
        )}

        {error ? <Text color="red">{error}</Text> : null}

        {reservations === null ? null : reservations.length === 0 ? (
          <Text color="gray">No reservations from today forward.</Text>
        ) : (
          <Card size="3">
            <Table.Root>
              <Table.Header>
                <Table.Row>
                  <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
                  <Table.ColumnHeaderCell>Guest email</Table.ColumnHeaderCell>
                  <Table.ColumnHeaderCell>Start</Table.ColumnHeaderCell>
                  <Table.ColumnHeaderCell>End</Table.ColumnHeaderCell>
                </Table.Row>
              </Table.Header>
              <Table.Body>
                {reservations.map((reservation) => (
                  <Table.Row key={reservation.id}>
                    <Table.RowHeaderCell>{reservation.roomNumber}</Table.RowHeaderCell>
                    <Table.Cell>{reservation.guestEmail}</Table.Cell>
                    <Table.Cell>{formatDate(reservation.start)}</Table.Cell>
                    <Table.Cell>{formatDate(reservation.end)}</Table.Cell>
                  </Table.Row>
                ))}
              </Table.Body>
            </Table.Root>
          </Card>
        )}
      </Flex>
    </Section>
  );
}

function formatDate(value: string) {
  return new Date(value).toLocaleDateString();
}
