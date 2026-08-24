def load_env(filename="config.env"):
    config = {}
    try:
        with open(filename) as f:
            for line in f:
                line = line.strip()
                if "=" in line and not line.startswith("#"):
                    key, value = line.split("=", 1)
                    config[key.strip()] = value.strip().strip("\"'")
    except OSError:
        print("Warning: No .env file found")
    return config

