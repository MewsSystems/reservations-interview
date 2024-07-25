import { Box, Grommet, Header, Heading } from "grommet";
import { LEFT_MEDIUM, THEME } from "./theme";
import { Outlet } from "@tanstack/react-router";

export const Layout = () => {
  return (
    <Grommet theme={THEME} full="min">
      <Header background="brand" pad={LEFT_MEDIUM}>
        <Heading level={1} size="small">
          Reservations @ Mewstel
        </Heading>
      </Header>
      <Box pad="large" height="full">
        <Outlet />
      </Box>
    </Grommet>
  );
};
