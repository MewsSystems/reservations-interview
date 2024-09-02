import * as Label from '@radix-ui/react-label';
import * as Checkbox from '@radix-ui/react-checkbox';
import { Box, Separator, Dialog, Button } from "@radix-ui/themes";

export type ReservationDetailsProps = {
    id: string,
    roomNumber: string,
    guestEmail: string,
    start: Date,
    end: Date,
    checkedIn: boolean,
    checkedOut: boolean,
    onSubmit: (props: ReservationDetailsProps) => void
}

export const ReservationDetailsModal = (props: ReservationDetailsProps) => {
    return (
        <Dialog.Content size="4">
            <Dialog.Title>Edit Reservation</Dialog.Title>
            <Dialog.Description>Room: #{props.roomNumber} | {props.start.toLocaleDateString()} - {props.end.toLocaleDateString()}</Dialog.Description>
            <Separator color="cyan" size="4" my="4" />
            <ReservationDetailsForm {...props} />
        </Dialog.Content>
    );
}

export const ReservationDetailsForm = (props: ReservationDetailsProps) => {

    function toggleCheckedIn() {
        console.log("toggle")
    }

    return (
        <Box style={{ position: "relative", minHeight: 700 }}>
            <Box mb="4">
                <Label.Root>Guest Email</Label.Root>
                <Box style={{ fontSize: '1rem', fontWeight: 'bold' }}>{props.guestEmail}</Box>
            </Box>

            <Box mb="4">
                <Label.Root>Room Number</Label.Root>
                <Box style={{ fontSize: '1rem', fontWeight: 'bold' }}>{props.roomNumber}</Box>
            </Box>

            <Box mb="4">
                <Label.Root>Start Date</Label.Root>
                <Box style={{ fontSize: '1rem', fontWeight: 'bold' }}>{props.start.toLocaleDateString()}</Box>
            </Box>

            <Box mb="4">
                <Label.Root>End Date</Label.Root>
                <Box style={{ fontSize: '1rem', fontWeight: 'bold' }}>{props.end.toLocaleDateString()}</Box>
            </Box>
        </Box>
    );
}