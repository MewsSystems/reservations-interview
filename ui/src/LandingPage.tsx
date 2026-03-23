import { Card, Flex, Heading, Inset } from "@radix-ui/themes";
import { Link } from "@tanstack/react-router";
import { useAuth } from "./utils/auth";
import { StaffLoginDialog } from "./staff/StaffLoginDialog";

const STAFF_IMG = "https://images.unsplash.com/photo-1550527882-b71dea5f8089?q=80&w=700&h=600&auto=format&fit=crop";
const RESERVE_IMG = "https://images.unsplash.com/photo-1531576788337-610fa9c67107?q=80&w=700&h=600&auto=format&fit=crop";

const IMG_STYLE: React.CSSProperties = {
  width: 350,
  height: 300,
  objectFit: "cover",
};

function LandingCard({ img, alt, label }: { img: string; alt: string; label: string }) {
  return (
    <>
      <Inset side="top" pb="current">
        <img src={img} alt={alt} style={IMG_STYLE} />
      </Inset>
      <Heading align="center">{label}</Heading>
    </>
  );
}

export function LandingPage() {
  const { isAuthenticated } = useAuth();

  return (
    <Flex direction="row" align="center" justify="center" gap="9" pt="9">
      {isAuthenticated ? (
        <Card size="3" asChild variant="classic">
          <Link to="/staff/reservations" preload="intent">
            <LandingCard img={STAFF_IMG} alt="Key on wood board" label="Reservations" />
          </Link>
        </Card>
      ) : (
        <StaffLoginDialog>
          <Card size="3" variant="classic" style={{ cursor: "pointer" }}>
            <LandingCard img={STAFF_IMG} alt="Key on wood board" label="Staff Login" />
          </Card>
        </StaffLoginDialog>
      )}
      <Card size="3" asChild variant="classic">
        <Link to="/reservations" preload="intent">
          <LandingCard img={RESERVE_IMG} alt="Clean Bed" label="Reserve" />
        </Link>
      </Card>
    </Flex>
  );
}
