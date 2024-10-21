import ky from "ky";
import { useEffect, useState } from "react";
import { Grid, Heading, Section, Dialog } from "@radix-ui/themes";

const RESPONSIVE_GRID_COLS: React.ComponentProps<typeof Grid>["columns"] = {
    sm: "1",
    md: "2",
    lg: "4",
};

export interface Reservation {
    id: string;
    roomNumber: string;
    guestEmail: string;
    start: string;
    end: string;
}

export function StaffPage() {
    const [reservations, setReservations] = useState<Reservation[]>([]);

    useEffect(() => {
        async function GetReservations() {
            try {
                const reservations = await ky.get("api/staff/reservations").json();
                setReservations(reservations as unknown as Reservation[]);
            } catch (error) {
                console.log("Check error:", error);
            }
        }

        GetReservations()
    }, []);

    return (
        <Section size="2" px="2">
            <Heading size="8" as="h1" color="mint">
                Reservations
            </Heading>

            <Grid columns={RESPONSIVE_GRID_COLS} gap="4" px="4" mt="8">
                <Dialog.Root>
                    {reservations?.map((reservation) => (
                        <div style={styles.card} key={reservation.id}>
                            <h3>Guest: {reservation.guestEmail}</h3>
                            <p>Room: {reservation.roomNumber}</p>
                            <p>Start Date: {reservation.start}</p>
                            <p>End Date: {reservation.end}</p>
                        </div>
                    ))}
                </Dialog.Root>
            </Grid>
        </Section>
    );
}

const styles = {
    card: {
        border: '1px solid #ccc',
        borderRadius: '8px',
        padding: '16px',
        marginBottom: '10px',
        backgroundColor: '#f9f9f9',
        width: '300px',
    },
};
