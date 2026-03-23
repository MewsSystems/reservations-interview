import { Box, Button, Dialog, Separator, Text } from "@radix-ui/themes";
import { Reservation } from "./api";

interface BookingConfirmationModalProps {
  reservation: Reservation;
  onClose: () => void;
}

export function BookingConfirmationModal({
  reservation,
  onClose,
}: BookingConfirmationModalProps) {
  const startDate = new Date(reservation.start).toLocaleDateString();
  const endDate = new Date(reservation.end).toLocaleDateString();

  return (
    <Dialog.Root open onOpenChange={(open) => !open && onClose()}>
      <Dialog.Content size="3">
        <Dialog.Title>Booking Confirmed</Dialog.Title>
        <Dialog.Description>
          Your reservation has been successful. We are happy to welcome you!
        </Dialog.Description>
        <Separator color="cyan" size="4" my="4" />
        <Box py="2">
          <Text as="div" size="3" mb="2">
            <strong>Confirmation ID:</strong> {reservation.id}
          </Text>
          <Text as="div" size="3" mb="2">
            <strong>Room:</strong> #{reservation.roomNumber}
          </Text>
          <Text as="div" size="3" mb="2">
            <strong>Email:</strong> {reservation.guestEmail}
          </Text>
          <Text as="div" size="3" mb="2">
            <strong>Check-in:</strong> {startDate}
          </Text>
          <Text as="div" size="3">
            <strong>Check-out:</strong> {endDate}
          </Text>
        </Box>
        <Separator color="cyan" size="4" my="4" />
        <Box style={{ display: "flex", justifyContent: "flex-end" }}>
          <Dialog.Close>
            <Button size="3" color="mint" onClick={onClose}>
              Done
            </Button>
          </Dialog.Close>
        </Box>
      </Dialog.Content>
    </Dialog.Root>
  );
}
