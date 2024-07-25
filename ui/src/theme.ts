import { ThemeType } from "grommet/themes/base";
import { EdgeType } from "grommet/utils";

export const THEME: ThemeType = {
  card: {
    hover: {
      container: {
        elevation: "large",
      },
    },
    container: {
      elevation: "medium",
      extend: `transition: all 0.2s ease-in-out;`,
    },
    footer: {
      pad: { horizontal: "medium", vertical: "small" },
      background: "#00000008",
    },
  },
  heading: {
    extend: {
      alignSelf: "center",
    },
  },
};

/** Left medium padding object constant */
export const LEFT_MEDIUM: EdgeType = {
  left: "medium",
};

export const RIGHT_MEDIUM: EdgeType = {
  right: "medium",
};
