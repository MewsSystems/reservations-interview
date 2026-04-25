import { useShowInfoToast } from "../utils/toasts";
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
  onSubmit: (booking: NewReservation) => void;
}

interface BookingFormProps {
  roomNumber: string;
  onSubmit: (booking: NewReservation) => void;
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
  const [focusedInput, setFocusedInput] = useState<FocusedInput | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);
  const showProcessingToast = useShowInfoToast("Processing booking...");
  const showNoInfoToast = useShowInfoToast("Missing valid email or dates.");

  const emailRegex = /^[^@\s]+@[^@\s]+\.[^@\s]+$/;
  const isValidEmail = emailRegex.test(email);
  const isEmailError = email.length > 0 && !isValidEmail;
  const isFormValid = isValidEmail && dateRange[0] !== null && dateRange[1] !== null;

  function handleSubmit(evt: React.MouseEvent<HTMLButtonElement>) {
    if (!isFormValid) {
      showNoInfoToast();
      evt.preventDefault();
      return false;
    }

    if (isSubmitting) {
      evt.preventDefault();
      return false;
    }

    setIsSubmitting(true);
    showProcessingToast();
    onSubmit({
      RoomNumber: roomNumber,
      GuestEmail: email,
      Start: dateRange[0],
      End: dateRange[1],
    });
    
    return true;
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
        color={isEmailError ? "red" : undefined}
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
        minBookingDate={new Date()}
      />
      <BottomRightBox>
        <Dialog.Close>
          <Button 
            size="3" 
            color="mint" 
            mt="4" 
            onClick={handleSubmit} 
            disabled={isSubmitting || !isFormValid}
          >
            Reserve
          </Button>
        </Dialog.Close>
      </BottomRightBox>
    </Box>
  );
}
