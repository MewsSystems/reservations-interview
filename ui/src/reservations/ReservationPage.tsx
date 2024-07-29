import { Box, Button, DateInput, Grid, Layer, Text, TextInput } from "grommet";
import {
  fromDateStringToIso,
  ISO8601String,
  toIsoStr,
} from "../utils/datetime";
import { ChangeEvent, useState } from "react";
import { postJson } from "../utils/api";
import { SuccessToast } from "../components/SuccessToast";

interface NewReservation {
  RoomNumber: number;
  GuestEmail: string;
  Start: ISO8601String;
  End: ISO8601String;
}

/** The type the API returns */
interface ReservationAPI {
  Id: string;
  RoomNumber: number;
  GuestEmail: string;
  Start: string;
  End: string;
}

function bookRoom(booking: NewReservation) {
  // unwrap branded types
  const newReservation = {
    ...booking,
    Start: toIsoStr(booking.Start),
    End: toIsoStr(booking.End),
  };

  // TODO impl reserving a room
  // ?? postJson?
  return Promise.resolve(newReservation);
}

export function ReservationPage() {
  const [showModal, setShowModal] = useState(false);
  const [selectedRoomNumber, setSelectedRoomNumber] = useState(-1);
  const [showSuccess, setShowSuccess] = useState(false);

  function onClose() {
    setShowModal(false);
    setShowSuccess(true);
  }

  function onSubmit(booking: NewReservation) {
    return bookRoom(booking).then(onClose);
  }

  const createClickHandler = (roomNumber: number) => () => {
    setSelectedRoomNumber(roomNumber);
    setShowModal(true);
  };

  function handleCloseToast() {
    setShowSuccess(false);
  }

  return (
    <>
      <Box>
        <Text size="4xl">Rooms</Text>
      </Box>
      <Grid justify="start" columns="1/4" gap="small">
        <Box
          hoverIndicator
          pad="small"
          border="all"
          onClick={createClickHandler(1)}
        >
          <Text>Room#001</Text>
          <img src="/bed.png" width={250}></img>
        </Box>

        <Box
          hoverIndicator
          pad="small"
          border="all"
          onClick={createClickHandler(2)}
        >
          <Text>Room#002</Text>
          <img src="/bed.png" width={250}></img>
        </Box>

        <Box
          hoverIndicator
          pad="small"
          border="all"
          onClick={createClickHandler(3)}
        >
          <Text>Room#003</Text>
          <img src="/bed.png" width={250}></img>
        </Box>

        <Box
          hoverIndicator
          pad="small"
          border="all"
          onClick={createClickHandler(4)}
        >
          <Text>Room#004</Text>
          <img src="/bed.png" width={250}></img>
        </Box>
      </Grid>
      <BookingDetailsModal
        roomNumber={selectedRoomNumber}
        show={showModal}
        onClose={onClose}
        onSubmit={onSubmit}
      />
      <SuccessToast
        show={showSuccess}
        onClose={handleCloseToast}
        message="We received your booking!"
      />
    </>
  );
}

interface BookingDetailsModalProps {
  roomNumber: number;
  show: boolean;
  onSubmit: (booking: NewReservation) => Promise<void>;
  onClose: () => void;
}

function BookingDetailsModal({
  roomNumber,
  show,
  onSubmit,
  onClose,
}: BookingDetailsModalProps) {
  const formattedRoomNumber = String(roomNumber).padStart(3, "0");

  const [email, setEmail] = useState("");
  function onEmailChange(event: ChangeEvent<HTMLInputElement>) {
    setEmail(event.target.value);
  }

  const [dateRange, setDateRange] = useState("");
  function onDateRangeChange(event: any) {
    setDateRange(event.value);
  }

  function handleClose() {
    resetForm();
    onClose();
  }

  function resetForm() {
    setEmail("");
    setDateRange("");
  }

  function handleSubmit() {
    const [start, end] = dateRange;

    onSubmit({
      RoomNumber: roomNumber,
      GuestEmail: email,
      Start: fromDateStringToIso(start),
      End: fromDateStringToIso(end),
    }).then(resetForm);
  }

  if (!show) {
    return null;
  }

  return (
    <Layer
      id="reservation"
      onClickOutside={handleClose}
      onEsc={handleClose}
      position="center"
    >
      <Box pad="medium" gap="small" width="large">
        <Text size="2xl">Booking for Room#{formattedRoomNumber}</Text>

        <Text size="lg" weight="bold">
          What is your email?
        </Text>

        <TextInput
          value={email}
          onChange={onEmailChange}
          aria-label="Input Email"
        />

        <Text size="lg" weight="bold">
          When is your stay?
        </Text>
        <DateInput
          id="booking_period"
          name="booking"
          defaultValue={[]}
          format="dd/mm/yyyy-dd/mm/yyyy"
          inline
          value={dateRange}
          onChange={onDateRangeChange}
        />
        <Box
          as="footer"
          align="center"
          justify="end"
          gap="medium"
          direction="row"
        >
          <Button
            onClick={handleClose}
            color="dark-3"
            type="reset"
            label="Close"
          />

          <Button onClick={handleSubmit} primary active label="Book" />
        </Box>
      </Box>
    </Layer>
  );
}
