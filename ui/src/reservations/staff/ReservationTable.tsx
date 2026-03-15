import { Table } from "@radix-ui/themes";

export function ReservationTable({ reservations, isLoading }) {
    if (isLoading) return <div>Loading...</div>;

    return (
        <Table.Root>
            <Table.Header>
                <Table.Row>
                    <Table.ColumnHeaderCell>Room</Table.ColumnHeaderCell>
                    <Table.ColumnHeaderCell>Email</Table.ColumnHeaderCell>
                    <Table.ColumnHeaderCell>Start</Table.ColumnHeaderCell>
                    <Table.ColumnHeaderCell>End</Table.ColumnHeaderCell>
                </Table.Row>
            </Table.Header>

            <Table.Body>
                {reservations.map((r) => (
                    <Table.Row key={r.id}>
                        <Table.Cell>{r.roomNumber}</Table.Cell>
                        <Table.Cell>{r.guestEmail}</Table.Cell>
                        <Table.Cell>{r.start}</Table.Cell>
                        <Table.Cell>{r.end}</Table.Cell>
                    </Table.Row>
                ))}
            </Table.Body>
        </Table.Root>
    );
}