import { useShowInfoToast } from "../utils/toasts";
import { fromDateStringToIso } from "../utils/datetime";
import {
  DateRangeInput,
  FocusedInput,
  OnDatesChangeProps,
} from "@datepicker-react/styled";
import { Box, Button, Dialog, Separator, TextField } from "@radix-ui/themes";
import { NewReservation } from "./api";
import { useCallback, useRef, useState } from "react";
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
  const emailFieldRef = useRef<HTMLInputElement>(null)
  const [email, setEmail] = useState("");
  const [dateRange, setDateRange] = useState<[Date | null, Date | null]>([
    null,
    null,
  ]);
  const [focusedInput, setFocusedInput] = useState<FocusedInput | null>(null);
  const showProcessingToast = useShowInfoToast("Processing booking...");
  const showInvalidEmailToast = useShowInfoToast("Missing or invalid email address.");
  const showInvalidDatesInfoToast = useShowInfoToast("Booking must be minimum 1 day or at most 10 days.");

  const validateBookingDates = useCallback(() => {
    const [startDate, endDate] = dateRange;
    if (!startDate || !endDate) {
      return false
    }
    const diffInMs = endDate.getTime() - startDate.getTime();
    // Check if number of days between 1 and 30
    const diffInDays = diffInMs / (1000 * 60 * 60 * 24);
    const validDateRange = diffInDays >= 0 && diffInDays <= 30;
    if (!validDateRange) {
      return false;
    }
    return true;
  }, [dateRange]);

  function handleSubmit(evt: React.MouseEvent<HTMLButtonElement>) {

    if (!email || !emailFieldRef?.current?.checkValidity()) {
      evt.preventDefault();
      showInvalidEmailToast();
      return false;
    }

    if (!validateBookingDates()) {
      evt.preventDefault();
      showInvalidDatesInfoToast();
      return false;
    }

    showProcessingToast();
    onSubmit({
      RoomNumber: roomNumber,
      GuestEmail: email,
      Start: fromDateStringToIso(dateRange[0] as Date),
      End: fromDateStringToIso(dateRange[1] as Date),
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
        ref={emailFieldRef}
        placeholder="... address@domain.tld ..."
        onChange={(evt) => setEmail(evt.target.value)}
        value={email}
        type="email"
        size="3"
        mb="4"
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
        <Dialog.Close>
          <Button size="3" color="mint" mt="4" onClick={handleSubmit}>
            Reserve
          </Button>
        </Dialog.Close>
      </BottomRightBox>
    </Box>
  );
}
