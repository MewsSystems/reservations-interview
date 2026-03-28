import { useShowInfoToast } from "../utils/toasts";
import { fromDateStringToIso } from "../utils/datetime";
import {
  DateRangeInput,
  FocusedInput,
  OnDatesChangeProps,
} from "@datepicker-react/styled";
import { Box, Button, Dialog, Separator, TextField } from "@radix-ui/themes";
import { NewReservation } from "./api";
import { useState } from "react";
import styled from "styled-components";

interface BookingDetailsModalProps {
  roomNumber: string;
  onSubmit: (booking: NewReservation) => Promise<void>;
}

interface BookingFormProps {
  roomNumber: string;
  onSubmit: (booking: NewReservation) => Promise<void>;
}

/** Must be inside a Dialog.Root that container Dialog.Triggers elsewhere */
export function BookingDetailsModal({
  roomNumber,
  onSubmit,
}: BookingDetailsModalProps) {
  return (
    <Dialog.Content size="4">
      <Dialog.Title>Booking Room #{roomNumber}</Dialog.Title>
      <Dialog.Description>
        Provide details for your reservation
      </Dialog.Description>
      <Separator color="cyan" size="4" my="4" />
      <BookingForm roomNumber={roomNumber} onSubmit={onSubmit} />
    </Dialog.Content>
  );
}

const DimSlot = styled(TextField.Slot)`
  background-color: var(--gray-4);
  margin-right: 8px;
`;

const BottomRightBox = styled(Box)`
  position: absolute;
  bottom: 0;
  right: 0;
`;

function BookingForm({ roomNumber, onSubmit }: BookingFormProps) {
  const [email, setEmail] = useState("");
  const [dateRange, setDateRange] = useState<[Date | null, Date | null]>([
    null,
    null,
  ]);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [focusedInput, setFocusedInput] = useState<FocusedInput | null>(null);
  const showProcessingToast = useShowInfoToast("Processing booking...");
  const showNoInfoToast = useShowInfoToast("Missing email or dates.");
  const showInvalidEmailToast = useShowInfoToast(
    "Enter an email with a domain.",
  );
  const showInvalidDateToast = useShowInfoToast(
    "Choose a stay from 1 to 30 days.",
  );

  async function handleSubmit(evt: React.MouseEvent<HTMLButtonElement>) {
    if (!email || !dateRange[0] || !dateRange[1]) {
      showNoInfoToast();
      evt.preventDefault();
      return;
    }

    if (!looksLikeEmailWithDomain(email)) {
      showInvalidEmailToast();
      evt.preventDefault();
      return;
    }

    const durationMs = dateRange[1].getTime() - dateRange[0].getTime();
    if (durationMs < ONE_DAY_MS || durationMs > MAX_DURATION_MS) {
      showInvalidDateToast();
      evt.preventDefault();
      return;
    }

    showProcessingToast();
    setIsSubmitting(true);

    try {
      await onSubmit({
        RoomNumber: roomNumber,
        GuestEmail: email.trim(),
        Start: fromDateStringToIso(dateRange[0]),
        End: fromDateStringToIso(dateRange[1]),
      });
    } finally {
      setIsSubmitting(false);
    }
  }

  function handleDateChange(data: OnDatesChangeProps) {
    if (data.startDate && data.endDate) {
      setDateRange([data.startDate, data.endDate]);
      setFocusedInput(null);
      return;
    }

    if (data.startDate) {
      setDateRange([data.startDate, null]);
      setFocusedInput("endDate");
      return;
    }

    setDateRange([dateRange[0] || null, data.endDate]);
    setFocusedInput("startDate");
  }

  return (
    <Box style={{ position: "relative", minHeight: 700 }}>
      <TextField.Root
        placeholder="... address@domain.tld ..."
        onChange={(evt) => setEmail(evt.target.value)}
        value={email}
        type="email"
        size="3"
        mb="4"
        disabled={isSubmitting}
      >
        <DimSlot side="left" prefix="email">
          Email
        </DimSlot>
      </TextField.Root>
      <DateRangeInput
        vertical
        showSelectedDates={false}
        placement="bottom"
        showStartDateCalendarIcon={false}
        showEndDateCalendarIcon={false}
        displayFormat="dd/MM/yyyy"
        onDatesChange={handleDateChange}
        startDate={dateRange[0]}
        endDate={dateRange[1]}
        focusedInput={focusedInput}
        onFocusChange={setFocusedInput}
        showResetDates={false}
      />
      <BottomRightBox>
        <Button
          size="3"
          color="mint"
          mt="4"
          onClick={handleSubmit}
          loading={isSubmitting}
          disabled={isSubmitting}
        >
          Reserve
        </Button>
      </BottomRightBox>
    </Box>
  );
}

const ONE_DAY_MS = 24 * 60 * 60 * 1000;
const MAX_DURATION_MS = ONE_DAY_MS * 30;

function looksLikeEmailWithDomain(email: string) {
  const [localPart, domain, ...rest] = email.trim().split("@");

  return (
    localPart.length > 0 &&
    domain !== undefined &&
    domain.includes(".") &&
    rest.length === 0
  );
}
