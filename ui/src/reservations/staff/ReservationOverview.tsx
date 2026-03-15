import { useState } from "react";
import { Box, Heading, Section } from "@radix-ui/themes";
import { useStaffReservations } from "../../reservations/api";
import { ReservationTable } from "./ReservationTable";
import { DateFilter } from "../../utils/datefilter";

export function StaffOverviewPage() {
    const [from, setFrom] = useState<string>();
    const [to, setTo] = useState<string>();

    const { data: reservations, isLoading } = useStaffReservations(from, to);

    return (
        <Section size="2">
            <Heading size="8">Reservations Overview</Heading>

            <DateFilter
                from={from}
                to={to}
                onFromChange={setFrom}
                onToChange={setTo}
            />

            <Box mt="6">
                <ReservationTable
                    reservations={reservations ?? []}
                    isLoading={isLoading}
                />
            </Box>
        </Section>
    );
}