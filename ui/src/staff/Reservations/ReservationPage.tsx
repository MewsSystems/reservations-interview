import { useState } from "react";
import { Grid, Heading, Section, Dialog, Button } from "@radix-ui/themes";
import { LoadingCard } from "../../components/LoadingCard";
import { patchReservation, useGetStaffReservations } from "../api";
import { ReservationCard } from "../../components/ReservationCard";
import { ReservationDetailsModal, ReservationDetailsProps } from "./ReservationDetailsModal";
import { useShowInfoToast, useShowSuccessToast } from "../../utils/toasts";

const RESPONSIVE_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
    sm: "1",
    md: "2",
    lg: "4",
};

export function StaffReservationPage() {
    const { isLoading, data: reservations, refetch } = useGetStaffReservations();
    const [activeFilter, toggleFilter] = useState(false);
    const [selectedReservation, setSelectedReservation] = useState<ReservationDetailsProps | null>(null);
    const showCheckinSuccess = useShowSuccessToast("Check in successful!");
    const showCheckinFailed = useShowInfoToast("Check in failed.");
    const showCannotCheckinDirty = useShowInfoToast("Cannot check in guest in dirty room.");

    function onClose() {
        setSelectedReservation(null);
    }

    function onCheckIn(reservation: ReservationDetailsProps) {
        if (reservation.state == 'Dirty') {
            showCannotCheckinDirty();
            return;
        }
        return patchReservation(reservation.id, reservation.guestEmail)
            .then((x) => {
                onClose();
                refetch();
                showCheckinSuccess()
            })
            .catch((x) => {
                onClose();
                showCheckinFailed()
            })
    }

    return (
        <Section size="2" px="2">
            <Heading size="8" as="h1" color="mint">
                Reservations
            </Heading>
            <Button onClick={() => toggleFilter(!activeFilter)}>{activeFilter ? "Show all reservations" : "Show reservations for today"}</Button>
            <Grid columns={RESPONSIVE_GRID_COLS} gap="4" px="4" mt="8">
                <Dialog.Root open={!!selectedReservation} onOpenChange={(open) => {
                    if (!open) {
                        onClose()
                    }
                }}>
                    {isLoading && <LoadingCard />}
                    {reservations?.filter((r) => activeFilter ? r.start.getDate() == new Date().getDate() : r).map((reservation) => (
                        <ReservationCard
                            key={reservation.id}
                            imgSrc="/bed.png"
                            title={`Room #${reservation.roomNumber}`}
                            subTitle={`${reservation.start.toLocaleDateString()} - ${reservation.end.toLocaleDateString()} - ${reservation.state}`}
                            onClick={() => {
                                if (selectedReservation != reservation)
                                    setSelectedReservation(reservation as ReservationDetailsProps)
                            }}
                        />
                    ))}
                    {selectedReservation ? <ReservationDetailsModal {...selectedReservation} onCheckIn={onCheckIn} /> : <></>}
                </Dialog.Root>
            </Grid>
        </Section>
    );
}
