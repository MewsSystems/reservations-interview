import { useState } from "react";
import { Grid, Heading, Section, Dialog } from "@radix-ui/themes";
import { LoadingCard } from "../../components/LoadingCard";
import { useGetStaffReservations } from "../api";
import { ReservationCard } from "../../components/ReservationCard";
import { ReservationDetailsModal, ReservationDetailsProps } from "./ReservationDetailsModal";

const RESPONSIVE_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
    sm: "1",
    md: "2",
    lg: "4",
};

export function StaffReservationPage() {
    const { isLoading, data: reservations } = useGetStaffReservations();
    const [selectedReservation, setSelectedReservation] = useState<ReservationDetailsProps | null>(null);

    function onSubmit(updatedReservation: ReservationDetailsProps) {
    }

    const createClickHandler = (reservation: ReservationDetailsProps) => () => {
        setSelectedReservation(reservation);
    };

    return (
        <Section size="2" px="2">
            <Heading size="8" as="h1" color="mint">
                Reservations
            </Heading>
            <Grid columns={RESPONSIVE_GRID_COLS} gap="4" px="4" mt="8">
                <Dialog.Root>
                    {isLoading && <LoadingCard />}
                    {reservations?.map((reservation) => (
                        <ReservationCard
                            key={reservation.id}
                            imgSrc="/bed.png"
                            title={`Room #${reservation.roomNumber}`}
                            subTitle={`${reservation.start.toLocaleDateString()} - ${reservation.end.toLocaleDateString()}`}
                            onClick={createClickHandler(reservation as ReservationDetailsProps)}
                        />
                    ))}
                    {selectedReservation ? <ReservationDetailsModal {...selectedReservation} onSubmit={onSubmit} /> : <></>}
                </Dialog.Root>
            </Grid>
        </Section>
    );
}
