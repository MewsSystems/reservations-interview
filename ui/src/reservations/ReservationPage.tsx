import {
  fromDateStringToIso,
  ISO8601String,
  toIsoStr,
} from "../utils/datetime";
import { ChangeEvent, useEffect, useState } from "react";
import { postJson } from "../utils/api";
import { useShowToast } from "../components/SuccessToast";
import { RoomCard } from "./RoomCard";
import Datepicker, { DateValueType } from "react-tailwindcss-datepicker";

function noop() {}

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

  const showToast = useShowToast("We have received your booking!");

  function onClose() {
    setShowModal(false);
    setSelectedRoomNumber(-1);
  }

  function onSubmit(booking: NewReservation) {
    return bookRoom(booking).then(onClose).then(showToast);
  }

  const createClickHandler = (roomNumber: number) => () => {
    setSelectedRoomNumber(roomNumber);
    setShowModal(true);
  };

  return (
    <div>
      <h1 className="text-4xl text-secondary mb-6">Rooms</h1>

      <div className="grid grid-cols-1 md:grid-cols-2 xl:grid-cols-4 pl-2 gap-4">
        <RoomCard
          imgSrc="/bed.png"
          roomNumber={1}
          onClick={createClickHandler(1)}
        />
        <RoomCard
          imgSrc="/bed.png"
          roomNumber={2}
          onClick={createClickHandler(2)}
        />
        <RoomCard
          imgSrc="/bed.png"
          roomNumber={3}
          onClick={createClickHandler(3)}
        />
        <RoomCard
          imgSrc="/bed.png"
          roomNumber={4}
          onClick={createClickHandler(4)}
        />
      </div>
      <BookingDetailsModal
        show={showModal}
        roomNumber={selectedRoomNumber}
        onSubmit={onSubmit}
        onClose={onClose}
      />
    </div>
  );
}

interface BookingDetailsModalProps {
  roomNumber: number;
  show: boolean;
  onSubmit: (booking: NewReservation) => Promise<any>;
  onClose: () => void;
}

function BookingDetailsModal({
  roomNumber,
  show,
  onSubmit,
  onClose,
}: BookingDetailsModalProps) {
  const formattedRoomNumber = String(roomNumber).padStart(3, "0");

  useEffect(() => {
    if (typeof ReservationDialog === "undefined") {
      return;
    }

    if (show) {
      ReservationDialog.showModal();
    } else {
      ReservationDialog.close();
    }
  }, [show]);

  const [email, setEmail] = useState("");
  function onEmailChange(event: ChangeEvent<HTMLInputElement>) {
    setEmail(event.target.value);
  }

  const [dateRange, setDateRange] = useState<DateValueType>({
    startDate: null,
    endDate: null,
  });
  function onDateRangeChange(newDateRange: DateValueType) {
    setDateRange(newDateRange);
  }

  function handleClose() {
    resetForm();
    onClose();
  }

  function resetForm() {
    setEmail("");
    setDateRange({ startDate: null, endDate: null });
  }

  function handleSubmit() {
    if (dateRange?.startDate == null || dateRange?.endDate == null) {
      return;
    }

    const { startDate, endDate } = dateRange;

    onSubmit({
      RoomNumber: roomNumber,
      GuestEmail: email,
      Start: fromDateStringToIso(startDate),
      End: fromDateStringToIso(endDate),
    }).then(resetForm);
  }

  if (!show) {
    return null;
  }

  return (
    <dialog
      id="ReservationDialog"
      className="modal modal-bottom md:modal-middle"
      onClose={handleClose}
    >
      <div className="modal-box relative px-6 md:w-[900px] md:max-w-[900px] h-[675px]">
        <button
          className="btn btn-sm btn-circle btn-ghost absolute right-4 md:right-2 top-2"
          onClick={handleClose}
        >
          ✕
        </button>
        <h3 className="font-bold text-lg mb-6">
          Booking for Room#{formattedRoomNumber}
        </h3>
        <label className="input input-bordered flex items-center gap-2 mb-4">
          <svg
            xmlns="http://www.w3.org/2000/svg"
            viewBox="0 0 16 16"
            fill="currentColor"
            className="h-4 w-4 opacity-70"
          >
            <path d="M2.5 3A1.5 1.5 0 0 0 1 4.5v.793c.026.009.051.02.076.032L7.674 8.51c.206.1.446.1.652 0l6.598-3.185A.755.755 0 0 1 15 5.293V4.5A1.5 1.5 0 0 0 13.5 3h-11Z" />
            <path d="M15 6.954 8.978 9.86a2.25 2.25 0 0 1-1.956 0L1 6.954V11.5A1.5 1.5 0 0 0 2.5 13h11a1.5 1.5 0 0 0 1.5-1.5V6.954Z" />
          </svg>
          <input
            type="email"
            value={email}
            onChange={onEmailChange}
            className="grow"
            placeholder="Email"
          />
        </label>
        <p className="mb-2">Booking Dates</p>
        <div className="input input-bordered flex items-center gap-2">
          <Datepicker
            value={dateRange}
            onChange={onDateRangeChange}
            inputClassName="w-full"
          />
        </div>
        <div className="modal-action absolute right-4 bottom-4 p-2">
          <button
            className="btn bg-secondary hover:bg-accent text-neutral hover:text-secondary"
            onClick={handleSubmit}
          >
            Book
          </button>
        </div>
      </div>
      <form method="dialog" className="modal-backdrop" onSubmit={handleClose}>
        <button>close</button>
      </form>
    </dialog>
  );
}

/** Once BookingDetailsModal renders, this global exists as the <dialog> has an id */
declare const ReservationDialog: HTMLDialogElement;
