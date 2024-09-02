import * as Label from '@radix-ui/react-label';
import { Box, Separator, Dialog, Button } from "@radix-ui/themes";
import { useEffect, useMemo, useState } from 'react';
import { getUserAccount } from '../api';

export type ReservationDetailsProps = {
    id: string,
    roomNumber: string,
    guestConfirmedAccount: boolean,
    guestEmail: string,
    start: Date,
    end: Date,
    checkedIn: boolean,
    checkedOut: boolean,
    onCheckIn: (props: ReservationDetailsProps) => void
}

export const ReservationDetailsModal = (props: ReservationDetailsProps) => {
    const [guestConfirmedAccount, setGuestConfirmedAccount] = useState<boolean | null>(null);

    useEffect(() => {
        const fetchUserAccount = async () => {
            try {
                const account = await getUserAccount(props.guestEmail);
                setGuestConfirmedAccount(account.isValidated ?? false);
            } catch (error) {
                setGuestConfirmedAccount(false);
            };
        }
        if (props.guestEmail) {
            fetchUserAccount();
        }
    }, [props.guestEmail])

    return (
        <Dialog.Content size="4">
            <Dialog.Title>Edit Reservation</Dialog.Title>
            <Dialog.Description>Room: #{props.roomNumber} | {props.start.toLocaleDateString()} - {props.end.toLocaleDateString()}</Dialog.Description>
            <Separator color="cyan" size="4" my="4" />
            <ReservationDetailsForm {...props} guestConfirmedAccount={guestConfirmedAccount ?? false} />
        </Dialog.Content>
    );
}

export const ReservationDetailsForm = (props: ReservationDetailsProps) => {
    var checkInDisabled = useMemo(() =>
        props.checkedIn || props.start.getDate() != new Date().getDate() || props.guestConfirmedAccount == false
        , [props.checkedIn || props.start, props.guestConfirmedAccount])

    return (
        <Box style={{ position: "relative", minHeight: 700 }}>
            <Box mb="4">
                <Label.Root>Guest Email</Label.Root>
                <Box style={{ fontSize: '1rem', fontWeight: 'bold' }}>{!props.guestConfirmedAccount ? "(Not confirmed) " : ""}{props.guestEmail}</Box>
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

            {!props.checkedIn ?
                <Button disabled={checkInDisabled}
                    style={{ cursor: !checkInDisabled ? 'pointer' : 'default' }}
                    onClick={() => props.onCheckIn(props)}>Check In user</Button>
                :
                (<Box mb="4">
                    <Label.Root>Checked In</Label.Root>
                    <Box style={{ fontSize: '1rem', fontWeight: 'bold' }}>Yes</Box>
                </Box>)}
        </Box>
    );
}