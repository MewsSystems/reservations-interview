export interface RoomCardProps {
  onClick: () => void;
  imgSrc: string;
  roomNumber: number;
}

export function RoomCard({ onClick, imgSrc, roomNumber }: RoomCardProps) {
  return (
    <div
      className="card w-[275px] h-[225px] bg-neutral group hover:bg-accent hover:cursor-pointer hover:ring-2"
      onClick={onClick}
    >
      <figure>
        <img src={imgSrc} alt="room photo" />
      </figure>
      <div className="card-body">
        <h2 className="card-title group-hover:text-secondary">
          Room#{roomNumber.toString().padStart(3, "0")}
        </h2>
      </div>
    </div>
  );
}
