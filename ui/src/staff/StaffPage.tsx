import { Box, Card, Flex, Heading, Section, Text, Button, Badge } from "@radix-ui/themes";
import { useState } from "react";
import { useGetStaffReservations, checkInGuest } from "../reservations/api";
import { useShowSuccessToast, useShowErrorToast } from "../utils/toasts";

export function StaffPage() {
  const [filterToday, setFilterToday] = useState(false);
  const { data: reservations, isLoading, refetch } = useGetStaffReservations();
  
  const showSuccess = useShowSuccessToast("Guest checked in!");
  const showError = useShowErrorToast();

  // Get current date in YYYY-MM-DD format for filtering
  const todayIso = new Date().toISOString().split('T')[0];

  const filteredReservations = filterToday 
    ? reservations?.filter(res => res.Start.startsWith(todayIso))
    : reservations;

  async function handleCheckIn(id: string, correctEmail: string) {
    const confirmation = prompt(`Please enter the guest email (${correctEmail}) to confirm check-in:`);
    
    if (!confirmation) return;

    try {
      await checkInGuest(id, confirmation);
      showSuccess();
      refetch(); 
    } catch (err: any) {
      const msg = await err.response?.text() || "Check-in failed.";
      showError(msg);
    }
  }

  if (isLoading) return <Section><Text>Loading...</Text></Section>;

  return (
    <Section size="2" px="4">
      <Flex justify="between" align="center" mb="6">
        <Heading size="8">Staff Dashboard</Heading>
        <Button 
          variant={filterToday ? "solid" : "outline"} 
          onClick={() => setFilterToday(!filterToday)}
        >
          {filterToday ? "Showing Today" : "Filter Today"}
        </Button>
      </Flex>

      <Flex direction="column" gap="3">
        {filteredReservations?.map((res) => {
        const isToday = res.Start.startsWith(todayIso);
        
        return (
            <Card key={res.Id  } size="2">
            <Flex justify="between" align="center">
                <Box>
                <Text as="div" weight="bold">Room #{res.RoomNumber}</Text>
                <Text as="div" size="2" color="gray">Guest: {res.GuestEmail}</Text>
                <Text as="div" size="1" color="gray">
                    {new Date(res.Start).toLocaleDateString()} - {new Date(res.End).toLocaleDateString()}
                </Text>
                </Box>

                <Flex align="center" gap="3">
                {res.CheckedIn ? (
                    <Badge color="green" size="2" variant="soft">
                    Checked In
                    </Badge>
                ) : (
                    isToday && (
                    <Button color="cyan" onClick={() => handleCheckIn(res.Id, res.GuestEmail)}>
                        Check In
                    </Button>
                    )
                )}
                </Flex>
            </Flex>
            </Card>
        );
    })}
      </Flex>
    </Section>
  );
}