import { Box, Card, Flex, Heading, Section, Text } from "@radix-ui/themes";
import { useGetStaffReservations } from "../reservations/api";

export function StaffPage() {
  const { data: reservations, isLoading, error } = useGetStaffReservations();

  if (isLoading) return <Text>Loading upcoming reservations...</Text>;
  if (error) return <Text color="red">Access Denied. Please log in again.</Text>;

  return (
    <Section size="2" px="4">
      <Heading size="8" mb="6">Staff Dashboard</Heading>
      <Heading size="4" mb="4" color="gray">Upcoming Reservations</Heading>
      
      <Flex direction="column" gap="3">
        {reservations?.map((res) => (
        <Card key={res.Id} size="2">
            <Flex justify="between" align="center">
            <Box>
                <Text as="div" weight="bold" size="4">Room #{res.RoomNumber}</Text>
                <Text as="div" size="2" color="gray">Guest: {res.GuestEmail}</Text>
            </Box>
            <Box>
        <Text as="div" size="2" align="right">
            {new Date(res.Start).toLocaleDateString()} to {new Date(res.End).toLocaleDateString()}
        </Text>
        </Box>
            </Flex>
        </Card>
        ))}
      </Flex>
    </Section>
  );
}