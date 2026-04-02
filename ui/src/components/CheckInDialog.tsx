import { useState, useEffect } from "react";
import {
  Box,
  Button,
  Dialog,
  Flex,
  Text,
  TextField,
} from "@radix-ui/themes";
import {
  initiateCheckIn,
  confirmCheckIn,
  type ReservationDetail,
} from "../reservations/api";
import { handleApiError, showErrorToast, showSuccessToast } from "../utils/toasts";

interface CheckInDialogProps {
  reservation: ReservationDetail | null;
  onClose: () => void;
  onConfirmed: () => void;
}

export function CheckInDialog({
  reservation,
  onClose,
  onConfirmed,
}: CheckInDialogProps) {
  const [generatedCode, setGeneratedCode] = useState<string | null>(null);
  const [codeInput, setCodeInput] = useState("");
  const [loading, setLoading] = useState(false);

  // Initiate check-in when a reservation is selected
  useEffect(() => {
    if (!reservation) return;
    setGeneratedCode(null);
    setCodeInput("");
    setLoading(true);

    initiateCheckIn(reservation.id)
      .then(setGeneratedCode)
      .catch(async (err) => {
        await handleApiError(err, "Failed to initiate check-in.");
        onClose();
      })
      .finally(() => setLoading(false));
  }, [reservation, onClose]);

  async function handleConfirm() {
    if (!reservation) return;
    setLoading(true);
    try {
      await confirmCheckIn(reservation.id, codeInput);
      showSuccessToast(
        `Checked in reservation for #${reservation.roomNumber}.`,
      );
      onConfirmed();
    } catch (err) {
      await handleApiError(err, "Invalid code or check-in failed.");
    } finally {
      setLoading(false);
    }
  }

  return (
    <Dialog.Root
      open={reservation !== null}
      onOpenChange={(open) => {
        if (!open) onClose();
      }}
    >
      <Dialog.Content maxWidth="440px">
        <Dialog.Title>Check In — #{reservation?.roomNumber}</Dialog.Title>

        <Text size="5" weight="bold" as="p" mb="1">
          {reservation?.guestEmail}
        </Text>

        {loading && !generatedCode && (
          <Text color="gray" size="2">
            Sending verification code...
          </Text>
        )}

        {generatedCode && (
          <>
            <Text size="2" as="p" mb="3">
              A verification code has been sent to the guest's email. Enter the
              code below to confirm check-in.
            </Text>

            <Box
              mb="3"
              p="2"
              style={{
                background: "var(--gray-a3)",
                borderRadius: "var(--radius-2)",
              }}
            >
              <Text size="1" color="gray" as="p" style={{ fontStyle: "italic" }}>
                Dev mock — code not emailed: <Text weight="bold">{generatedCode}</Text>
              </Text>
            </Box>

            <TextField.Root
              placeholder="Enter verification code..."
              size="3"
              value={codeInput}
              onChange={(e) => setCodeInput(e.target.value)}
            />
            <Flex justify="end" gap="2" mt="4">
              <Dialog.Close>
                <Button variant="soft" color="gray">
                  Cancel
                </Button>
              </Dialog.Close>
              <Button
                color="green"
                disabled={!codeInput.trim() || loading}
                onClick={handleConfirm}
              >
                {loading ? "Confirming..." : "Confirm Check In"}
              </Button>
            </Flex>
          </>
        )}
      </Dialog.Content>
    </Dialog.Root>
  );
}
